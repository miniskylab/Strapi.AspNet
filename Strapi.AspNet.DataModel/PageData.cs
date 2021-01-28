using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Strapi.AspNet.Annotations;

namespace Strapi.AspNet.DataModel
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public abstract class PageData : ContentData
    {
        [StrapiReserved]
        [JsonProperty("id")]
        public int PageId { get; private set; }

        [StrapiReserved]
        [JsonProperty("created_by")]
        public User CreatedBy { get; private set; }

        [StrapiReserved]
        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; private set; }

        [StrapiReserved]
        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; private set; }

        [StrapiReserved]
        [JsonProperty("published_at")]
        public DateTimeOffset? PublishedAt { get; private set; }

        [Required]
        [Display(Name = "Name", Order = 1)]
        public string PageInstanceName { get; private set; }

        [Display(Name = "Name In Url", Order = 2)]
        [UidFor(nameof(PageInstanceName))]
        public string NameInUrl { get; private set; }

        [Required]
        [Display(Name = "Meta Description", Order = 3)]
        public string MetaDescription { get; private set; }

        [Required]
        [Display(Name = "Page Title", Order = 4)]
        public string PageTitle { get; private set; }

        public override void SetDefaultValues() { PageInstanceName = "New Page"; }
    }
}