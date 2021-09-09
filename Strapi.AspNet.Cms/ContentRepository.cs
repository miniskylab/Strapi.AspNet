using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MiniSkyLab.Core;
using Strapi.AspNet.Cms.Abstractions;
using Strapi.AspNet.DataModel;
using Strapi.AspNet.Engine;

namespace Strapi.AspNet.Cms
{
    [UsedImplicitly]
    internal class ContentRepository : IContentRepository
    {
        public ContentRepository(IAppSettings appSettings, IStrapiAdmin strapiAdmin, IHttpClient httpClient,
            IAssemblyScanner assemblyScanner)
        {
            _httpClient = httpClient;
            _strapiAdmin = strapiAdmin;
            _assemblyScanner = assemblyScanner;

            strapiAdmin.Authorize(_httpClient);
        }

        public PageData GetPageData(string nameInUrl)
        {
            var dotnetPageTypes = _assemblyScanner.Types.Where(x =>
                x.IsClass &&
                !x.IsNested &&
                !x.IsAbstract &&
                x.IsSubclassOf(typeof(PageData))
            );

            foreach (var dotnetPageType in dotnetPageTypes)
            {
                var queryString = $"?{nameof(PageData.NameInUrl)}={nameInUrl}";
                var correspondingStrapiTypeId = dotnetPageType.GetCorrespondingStrapiTypeId();
                var contentUri = $"{_strapiAdmin.ContentManagerUrl}/collection-types/{correspondingStrapiTypeId}{queryString}";
                var httpResponseData = _httpClient.SendHttpRequest(HttpMethod.Get, contentUri);
                var httpResponseJObject = JObject.Parse(httpResponseData);
                var pageDataCollection = JsonConvert.DeserializeObject(
                    httpResponseJObject.GetToken("results").ToString(),
                    dotnetPageType.MakeArrayType(),
                    new JsonSerializerSettings
                    {
                        ContractResolver = new PrivateSetterContractResolver(),
                        Converters = new List<JsonConverter>
                        {
                            new AssumedUtcDateTimeJsonConverter(),
                            new JavaScriptBooleanJsonConverter()
                        }
                    }
                ) as PageData[];

                if (pageDataCollection == null)
                {
                    continue;
                }

                var publishedPageDataCollectionWithPaginationInfo = new PaginatedPageDataCollection(
                    httpResponseJObject.GetToken("pagination").ToObject<PaginationInfo>(),
                    pageDataCollection.Where(x => x.PublishedAt != null).ToArray()
                );

                return publishedPageDataCollectionWithPaginationInfo.PageDataCollection.SingleOrDefault();
            }

            return null;
        }

        #region Injected Services

        readonly IHttpClient _httpClient;
        readonly IStrapiAdmin _strapiAdmin;
        readonly IAssemblyScanner _assemblyScanner;

        #endregion
    }
}