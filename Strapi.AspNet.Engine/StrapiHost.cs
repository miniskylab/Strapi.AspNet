using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Orbital.Core;

namespace Strapi.AspNet.Engine
{
    [UsedImplicitly]
    internal class StrapiHost : IStrapiHost
    {
        public StrapiHost(IHttpClient httpClient, IStrapiAdmin strapiAdmin, IStrapiEditorUi strapiEditorUi, IStrapiProcess strapiProcess,
            IStrapiBuilder strapiBuilder, ILogger<StrapiHost> logger)
        {
            _logger = logger;
            _httpClient = httpClient;
            _strapiAdmin = strapiAdmin;
            _strapiProcess = strapiProcess;
            _strapiBuilder = strapiBuilder;
            _strapiEditorUi = strapiEditorUi;
        }

        public void Start()
        {
            _strapiProcess.Start();

            if (!_strapiAdmin.HasAdmin())
            {
                _strapiAdmin.RegisterDefaultAdmin();
                _strapiProcess.MarkAsInitializedSuccessfully();
            }

            _strapiAdmin.Authorize(_httpClient);
            _strapiBuilder.BuildStrapiTypesFromDotnetTypes();
            _strapiEditorUi.Configure();

            _logger.LogInformation($"Admin UI Uri: {_strapiAdmin.Url}");
        }

        #region Injected Services

        readonly ILogger _logger;
        readonly IHttpClient _httpClient;
        readonly IStrapiAdmin _strapiAdmin;
        readonly IStrapiProcess _strapiProcess;
        readonly IStrapiEditorUi _strapiEditorUi;
        readonly IStrapiBuilder _strapiBuilder;

        #endregion
    }
}