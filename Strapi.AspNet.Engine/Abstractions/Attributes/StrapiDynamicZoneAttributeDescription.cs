using Newtonsoft.Json;

namespace Strapi.AspNet.Engine
{
    internal class StrapiDynamicZoneAttributeDescription : StrapiAttributeDescription
    {
        [JsonProperty("components")]
        public string[] AllowedComponents { get; private set; }

        [JsonProperty("max")]
        public int? MaxComponentCount { get; private set; }

        [JsonProperty("min")]
        public int? MinComponentCount { get; private set; }

        [JsonProperty("required")]
        public bool? IsRequired { get; private set; }

        public StrapiDynamicZoneAttributeDescription(string[] allowedComponents, int? minComponentCount, int? maxComponentCount,
            bool? isRequired)
        {
            Type = "dynamiczone";
            AllowedComponents = allowedComponents;
            MinComponentCount = minComponentCount;
            MaxComponentCount = maxComponentCount;
            IsRequired = isRequired;
        }
    }
}