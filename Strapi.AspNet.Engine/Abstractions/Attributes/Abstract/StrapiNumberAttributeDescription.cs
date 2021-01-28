using Newtonsoft.Json;

namespace Strapi.AspNet.Engine
{
    internal abstract class StrapiNumberAttributeDescription<TDefaultValueType> : StrapiPrimitiveAttributeDescription<TDefaultValueType>
    {
        [JsonProperty("min")]
        public int? MinValue { get; private set; }

        [JsonProperty("max")]
        public int? MaxValue { get; private set; }

        protected StrapiNumberAttributeDescription(TDefaultValueType defaultValue, int? minValue, int? maxValue, bool? isPrivate,
            bool? isUnique) : base(defaultValue, isPrivate, true, isUnique)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }
    }
}