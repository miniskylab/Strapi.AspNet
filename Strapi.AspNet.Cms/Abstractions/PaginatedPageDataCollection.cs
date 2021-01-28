using Newtonsoft.Json;
using Strapi.AspNet.DataModel;

namespace Strapi.AspNet.Cms.Abstractions
{
    internal class PaginatedPageDataCollection
    {
        public PaginationInfo PaginationInfo { get; }

        [JsonProperty("results")]
        public PageData[] PageDataCollection { get; }

        public PaginatedPageDataCollection(PaginationInfo paginationInfo, PageData[] pageDataCollection)
        {
            PaginationInfo = paginationInfo;
            PageDataCollection = pageDataCollection;
        }
    }

    internal class PaginationInfo
    {
        [JsonProperty("page")]
        public int CurrentPageNo { get; private set; }

        [JsonProperty("pageCount")]
        public int PageCount { get; private set; }

        [JsonProperty("pageSize")]
        public int ItemCountPerPage { get; private set; }

        [JsonProperty("total")]
        public int TotalItemCount { get; private set; }
    }
}