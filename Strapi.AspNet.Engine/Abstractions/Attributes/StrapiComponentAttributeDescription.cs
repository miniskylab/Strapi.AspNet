using Newtonsoft.Json;

namespace Strapi.AspNet.Engine
{
    internal class StrapiComponentAttributeDescription : StrapiAttributeDescription
    {
        [JsonProperty("component")]
        public string Component { get; private set; }

        [JsonProperty("repeatable")]
        public bool Repeatable { get; private set; }

        [JsonProperty("max")]
        public int? MaxComponentCount { get; private set; }

        [JsonProperty("min")]
        public int? MinComponentCount { get; private set; }

        [JsonProperty("required")]
        public bool? IsRequired { get; private set; }

        public StrapiComponentAttributeDescription(string component, bool repeatable, int? minComponentCount, int? maxComponentCount,
            bool? isRequired)
        {
            Type = "component";
            Component = component;
            Repeatable = repeatable;
            IsRequired = isRequired;
            MinComponentCount = minComponentCount;
            MaxComponentCount = maxComponentCount;
        }
    }
}