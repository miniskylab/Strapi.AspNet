using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using Orbital.Core;
using Strapi.AspNet.DataModel;

namespace Strapi.AspNet.Engine
{
    [UsedImplicitly]
    internal class StrapiRepository : DbContext, IStrapiRepository
    {
        public StrapiRepository(IHttpClient httpClient, IStrapiAdmin strapiAdmin, IAssemblyScanner assemblyScanner,
            IAppSettings appSettings)
        {
            _httpClient = httpClient;
            _strapiAdmin = strapiAdmin;
            _assemblyScanner = assemblyScanner;

            strapiAdmin.Authorize(_httpClient);

            if (!appSettings.GetSection("Strapi:MySql").Exists())
            {
                return;
            }

            using var strapiDbContext = new StrapiDbContext(appSettings);
            strapiDbContext.Database.EnsureCreated();
        }

        public IEnumerable<StrapiComponentTypeMetadata> GetStrapiComponentTypeMetadata()
        {
            var componentTypeBuilderUri = $"{_strapiAdmin.BaseUrl}/content-type-builder/components";
            var httpResponseData = _httpClient.SendHttpRequest(HttpMethod.Get, componentTypeBuilderUri);

            var jArray = (JArray) JObject.Parse(httpResponseData).GetToken("data");
            var dotnetBlockTypes = _assemblyScanner.Types.Where(x => !x.IsAbstract && x.IsSubclassOf(typeof(BlockData))).ToList();

            return (
                from jChildObject in jArray.Children<JObject>()
                let strapiComponentTypeApiId = (string) jChildObject.GetToken("apiId")
                let strapiComponentTypeUid = (string) jChildObject.GetToken("uid")
                let identityMatch = new Func<Type, bool>(x => x.GetGuid() == strapiComponentTypeApiId)
                let correspondingDotnetBlockType = dotnetBlockTypes.SingleOrDefault(identityMatch)
                select new StrapiComponentTypeMetadata(strapiComponentTypeUid, strapiComponentTypeApiId, correspondingDotnetBlockType)
            ).ToArray();
        }

        public IEnumerable<StrapiContentTypeMetadata> GetStrapiContentTypeMetadata()
        {
            var contentTypeBuilderUri = $"{_strapiAdmin.BaseUrl}/content-type-builder/content-types";
            var httpResponseData = _httpClient.SendHttpRequest(HttpMethod.Get, contentTypeBuilderUri);

            var jArray = (JArray) JObject.Parse(httpResponseData).GetToken("data");
            var dotnetPageTypes = _assemblyScanner.Types.Where(x => x.IsSubclassOf(typeof(PageData))).ToList();

            return (
                from jChildObject in jArray.Children<JObject>()
                let strapiContentTypeUid = (string) jChildObject.GetToken("uid")
                let isContentTypeMappedFromDotnetType = strapiContentTypeUid != null && strapiContentTypeUid.StartsWith("application::")
                where isContentTypeMappedFromDotnetType
                let strapiContentTypeApiId = (string) jChildObject.GetToken("apiID")
                let identityMatch = new Func<Type, bool>(x => x.GetGuid() == strapiContentTypeApiId)
                let correspondingDotnetPageType = dotnetPageTypes.SingleOrDefault(identityMatch)
                select new StrapiContentTypeMetadata(strapiContentTypeUid, strapiContentTypeApiId, correspondingDotnetPageType)
            ).ToArray();
        }

        class StrapiDbContext : DbContext
        {
            readonly IAppSettings _appSettings;

            public StrapiDbContext(IAppSettings appSettings) { _appSettings = appSettings; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseMySql(new MySqlConnectionStringBuilder
                {
                    Database = _appSettings.Get("Strapi:MySql:Database"),
                    UserID = _appSettings.Get("Strapi:MySql:Username"),
                    Password = _appSettings.Get("Strapi:MySql:Password"),
                    Server = _appSettings.Get("Strapi:MySql:IpAddress"),
                    Port = _appSettings.Get<uint>("Strapi:MySql:Port"),
                    ConnectionTimeout = 3600
                }.ToString());
            }
        }

        #region Injected Services

        readonly IHttpClient _httpClient;
        readonly IStrapiAdmin _strapiAdmin;
        readonly IAssemblyScanner _assemblyScanner;

        #endregion
    }
}