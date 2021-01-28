using Newtonsoft.Json;

namespace Strapi.AspNet.Engine
{
    internal abstract class StrapiPrimitiveAttributeDescription : StrapiAttributeDescription
    {
        [JsonProperty("private")]
        public bool? IsPrivate { get; protected set; }

        [JsonProperty("required")]
        public bool? IsRequired { get; protected set; }

        [JsonProperty("unique")]
        public bool? IsUnique { get; protected set; }

        protected StrapiPrimitiveAttributeDescription(bool? isPrivate, bool? isRequired, bool? isUnique)
        {
            IsPrivate = isPrivate;
            IsRequired = isRequired;
            IsUnique = isUnique;
        }
    }

    internal abstract class StrapiPrimitiveAttributeDescription<TDefaultValueType> : StrapiPrimitiveAttributeDescription
    {
        [JsonProperty("default")]
        public TDefaultValueType DefaultValue { get; private set; }

        protected StrapiPrimitiveAttributeDescription(TDefaultValueType defaultValue, bool? isPrivate, bool? isRequired, bool? isUnique)
            : base(isPrivate, isRequired, isUnique)
        {
            DefaultValue = defaultValue;
        }
    }
}