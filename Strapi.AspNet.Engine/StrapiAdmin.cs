using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using MiniSkyLab.Core;
using Strapi.AspNet.DataModel;

namespace Strapi.AspNet.Engine
{
    [UsedImplicitly]
    internal class StrapiAdmin : IStrapiAdmin
    {
        static readonly string Secret;
        readonly string _jwtToken;

        public string Url { get; }

        public string Path { get; }

        public string BaseUrl { get; }

        public string ContentManagerUrl { get; }

        string IStrapiAdmin.JwtSecret => Secret;

        static StrapiAdmin()
        {
            byte[] jwtSecretByteArray;
            RandomNumberGenerator.Create().GetBytes(jwtSecretByteArray = new byte[128]);
            Secret = Convert.ToBase64String(jwtSecretByteArray);
        }

        public StrapiAdmin(ILogger<StrapiAdmin> logger, IAppSettings appSettings, IHttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;

            var port = appSettings.Get("Strapi:Port");
            BaseUrl = $"http://localhost:{port}";
            ContentManagerUrl = $"{BaseUrl}/content-manager";

            Path = appSettings.Get("Strapi:AdminPath");
            Url = $"{BaseUrl}{Path}";

            _jwtToken = GenerateJwtToken(TimeSpan.FromDays(36500));
        }

        public void Authorize(IHttpClient httpClient)
        {
            httpClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse($"Bearer {_jwtToken}");
        }

        bool IStrapiAdmin.HasAdmin()
        {
            var strapiAdminInfoUri = $"{BaseUrl}/admin/init";
            var httpResponseData = _httpClient.SendHttpRequest(HttpMethod.Get, strapiAdminInfoUri);

            return (bool) JObject.Parse(httpResponseData).GetToken("data.hasAdmin");
        }

        void IStrapiAdmin.RegisterDefaultAdmin()
        {
            var strapiAdminRegistrationUri = $"{BaseUrl}/admin/register-admin";
            var strapiDefaultAdminRegistrationDto = new StrapiDefaultAdminRegistrationDto();

            _httpClient.SendHttpRequest(HttpMethod.Post, strapiAdminRegistrationUri, strapiDefaultAdminRegistrationDto.ToJson());
            _logger.LogInformation(
                "Default admin registered. Username: {Email} - Password: {Password}",
                strapiDefaultAdminRegistrationDto.Email,
                strapiDefaultAdminRegistrationDto.Password
            );
        }

        static string GenerateJwtToken(TimeSpan duration)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var jwtToken = new JwtSecurityToken(
                new JwtHeader(credentials),
                new JwtPayload
                {
                    { "id", 1 },
                    { "isAdmin", true },
                    { "iat", DateTimeOffset.Now.ToUnixTimeMilliseconds() },
                    { "exp", DateTimeOffset.Now.Add(duration).ToUnixTimeMilliseconds() }
                }
            );

            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            return jwtSecurityTokenHandler.WriteToken(jwtToken);
        }

        #region Injected Services

        readonly ILogger _logger;
        readonly IHttpClient _httpClient;

        #endregion
    }
}