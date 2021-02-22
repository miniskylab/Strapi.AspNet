using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Orbital.Core;

namespace Strapi.AspNet.Engine
{
    [UsedImplicitly]
    internal class StrapiProcess : IStrapiProcess
    {
        readonly Process _process;
        readonly string _pathToStrapiInitializationMarkerFile;

        public StrapiProcess(ILogger<StrapiProcess> logger, IHttpClient httpClient, IStrapiAdmin strapiAdmin, IAppSettings appSettings)
        {
            _logger = logger;
            _httpClient = httpClient;
            _strapiAdmin = strapiAdmin;
            _appSettings = appSettings;

            _pathToStrapiInitializationMarkerFile = Path.Combine(
                _appSettings.PathToWorkingDirectory,
                "strapi", "node_modules", "strapi.initialized"
            );

            _process = CreateAndConfigureStrapiProcess();
        }

        public void MarkAsInitializedSuccessfully()
        {
            File.AppendAllText(_pathToStrapiInitializationMarkerFile, string.Empty);
            File.SetAttributes(_pathToStrapiInitializationMarkerFile, FileAttributes.Hidden);
        }

        public void Start()
        {
            if (_process.IsRunning())
                throw new InvalidOperationException("Strapi process is already running.");

            try
            {
                EnsureFreshStart();

                _process.Start();
                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();

                WaitForStrapiProcessToBeReady();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "One or more errors occurred while starting Strapi process");
            }
        }

        public void Restart()
        {
            Shutdown();
            Start();
        }

        void Shutdown()
        {
            if (!_process.IsRunning())
                throw new InvalidOperationException("Strapi process is not running.");

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    FixProcessTerminationIssuesOnWindows();
                }
                else
                {
                    _process.Kill(true);
                    _process.WaitForExit();
                }

                EnsureShutdown();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "One or more errors occurred while shutting down Strapi process");
            }
        }

        void EnsureShutdown()
        {
            try
            {
                _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, _strapiAdmin.Url)).Wait();
                throw new SystemException("Strapi process could not be stopped.");
            }
            catch (Exception exception)
            {
                if (exception is HttpRequestException || exception is AggregateException { InnerException: HttpRequestException })
                {
                    /* A HttpRequestException being thrown indicates that no connection could be made to Strapi process.
                       That means the Strapi process has been fully shutdown. */
                    return;
                }

                throw;
            }
        }

        void EnsureFreshStart()
        {
            try
            {
                _process.CancelOutputRead();
                _process.CancelErrorRead();
                _process.Refresh();
            }
            catch (InvalidOperationException)
            {
                /* The InvalidOperationException will be thrown if we haven't begun reading from Error or Output stream.
                   We just want to ensure we cancelled the previous read operations. So, we ignore the exception. */
            }
        }

        void WaitForStrapiProcessToBeReady()
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(1000);

                    var httpResponseMessage = _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, _strapiAdmin.Url)).Result;
                    if (httpResponseMessage.IsSuccessStatusCode)
                        break;
                }
                catch (Exception exception)
                {
                    if (exception is HttpRequestException || exception is AggregateException { InnerException: HttpRequestException })
                    {
                        /* A HttpRequestException being thrown indicates that no connection could be made to Strapi process.
                           We ignore this exception and let the loop continue, keep waiting for Strapi process to be ready. */
                        continue;
                    }

                    throw;
                }
            }
        }

        void FixProcessTerminationIssuesOnWindows()
        {
            /* HACK:
             * "_process.Kill(true)" doesn't work on Windows. It doesn't terminate the entire process tree as we expect.
             * Thus, we need to manually locate and terminate all child processes of the "cmd" process we start.
             */

            var childProcesses = GetChildProcesses(_process);
            static Process[] GetChildProcesses(Process parentProcess)
            {
                var childProcesses = new List<Process>();

                // After some experiments, we learned that only "cmd" and "node" processes need to be considered.
                var possibleChildProcesses = new List<Process>();
                possibleChildProcesses.AddRange(Process.GetProcessesByName("cmd"));
                possibleChildProcesses.AddRange(Process.GetProcessesByName("node"));

                foreach (var childProcess in possibleChildProcesses)
                {
                    var parentProcessIdProperty = childProcess.GetType()
                        .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetProperty)
                        .Single(x => x.Name == "ParentProcessId");

                    if (childProcess.HasExited)
                    {
                        continue;
                    }

                    var parentProcessId = parentProcessIdProperty.GetValue(childProcess) as int?;
                    if (parentProcessId != parentProcess.Id)
                    {
                        continue;
                    }

                    childProcesses.Add(childProcess);
                    childProcesses.AddRange(GetChildProcesses(childProcess));
                }

                return childProcesses.ToArray();
            }

            _process.Kill(true);
            foreach (var childProcess in childProcesses.Where(x => !x.HasExited))
            {
                childProcess.Kill(true);
                childProcess.WaitForExit();
            }
            _process.WaitForExit();
        }

        Process CreateAndConfigureStrapiProcess()
        {
            var argumentPrepend = "-c";
            var shellName = "/bin/bash";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                shellName = "cmd";
                argumentPrepend = "/c";
            }

            var strapiInitializedSuccessfully = File.Exists(_pathToStrapiInitializationMarkerFile);
            var cmdNpmInstall = !strapiInitializedSuccessfully ? "npm install --depth 0" : string.Empty;
            var cmdBuildStrapiAdminUi = !strapiInitializedSuccessfully ? "&& npm run build" : string.Empty;
            var cmdStartStrapi = $"{(!strapiInitializedSuccessfully ? "&&" : string.Empty)} npm run start";

            var localBlobStorageDirectory = "./public";
            if (_appSettings.GetSection("Strapi:BlobStorage:Local:Directory").Exists())
            {
                localBlobStorageDirectory = _appSettings.Get("Strapi:BlobStorage:Local:Directory");
                Directory.CreateDirectory(Path.Combine(localBlobStorageDirectory, "uploads"));
            }

            var strapiProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    FileName = shellName,
                    WorkingDirectory = Path.Combine(_appSettings.PathToWorkingDirectory, "strapi"),
                    Arguments = $"{argumentPrepend} \"{cmdNpmInstall} {cmdBuildStrapiAdminUi} {cmdStartStrapi}\"",
                    EnvironmentVariables =
                    {
                        { "HOST", _appSettings.Get("Strapi:IpAddress") },
                        { "PORT", _appSettings.Get("Strapi:Port") },
                        { "ADMIN_URL", _strapiAdmin.Path },
                        { "JWT_SECRET", _strapiAdmin.JwtSecret },
                        { "LOCAL_BLOB_STORAGE_DIRECTORY", localBlobStorageDirectory },
                        { "STRAPI_HIDE_STARTUP_MESSAGE", "true" },
                        { "STRAPI_TELEMETRY_DISABLED", "true" },
                        { "STRAPI_LOG_PRETTY_PRINT", "true" },
                        { "STRAPI_LOG_FORCE_COLOR", "false" },
                        { "NODE_ENV", "production" }
                    }
                }
            };

            if (_appSettings.GetSection("Strapi:MySql").Exists())
            {
                strapiProcess.StartInfo.EnvironmentVariables["DATABASE_CLIENT"] = "mysql";
                strapiProcess.StartInfo.EnvironmentVariables["DATABASE_NAME"] = _appSettings.Get("Strapi:MySql:Database");
                strapiProcess.StartInfo.EnvironmentVariables["DATABASE_HOST"] = _appSettings.Get("Strapi:MySql:IpAddress");
                strapiProcess.StartInfo.EnvironmentVariables["DATABASE_PORT"] = _appSettings.Get("Strapi:MySql:Port");
                strapiProcess.StartInfo.EnvironmentVariables["DATABASE_USERNAME"] = _appSettings.Get("Strapi:MySql:Username");
                strapiProcess.StartInfo.EnvironmentVariables["DATABASE_PASSWORD"] = _appSettings.Get("Strapi:MySql:Password");
            }
            else
            {
                strapiProcess.StartInfo.EnvironmentVariables["DATABASE_CLIENT"] = "sqlite";
                strapiProcess.StartInfo.EnvironmentVariables["DATABASE_FILENAME"] = ".tmp/data.db";
                strapiProcess.StartInfo.EnvironmentVariables["DATABASE_USE_NULL_AS_DEFAULT"] = "true";
            }

            void OnDataReceived(object sender, DataReceivedEventArgs args)
            {
                var data = args.Data;
                if (string.IsNullOrWhiteSpace(data))
                {
                    return;
                }

                data = Regex.Replace(data, @"^\[.+\] ", string.Empty);
                if (string.IsNullOrWhiteSpace(data))
                {
                    return;
                }

                _logger.LogDebug(data);
            }
            strapiProcess.OutputDataReceived += OnDataReceived;
            strapiProcess.ErrorDataReceived += OnDataReceived;

            return strapiProcess;
        }

        #region Injected Services

        readonly ILogger _logger;
        readonly IHttpClient _httpClient;
        readonly IStrapiAdmin _strapiAdmin;
        readonly IAppSettings _appSettings;

        #endregion
    }
}