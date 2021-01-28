using Newtonsoft.Json;

namespace Strapi.AspNet.Engine
{
    internal abstract class StrapiAttributeDescription
    {
        [JsonProperty("type")]
        public string Type { get; protected set; }

        public virtual StrapiAttributeDescription ToStrapiAttributeModel() { return this; }
    }
}