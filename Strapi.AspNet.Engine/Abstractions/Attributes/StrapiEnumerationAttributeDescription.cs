using System.Collections.Generic;
using Newtonsoft.Json;

namespace Strapi.AspNet.Engine
{
    internal class StrapiEnumerationAttributeDescription : StrapiPrimitiveAttributeDescription<string>
    {
        [JsonProperty("enum")]
        public IEnumerable<string> Enum { get; private set; }

        public StrapiEnumerationAttributeDescription(string defaultValue, IEnumerable<string> @enum, bool? isPrivate, bool? isRequired,
            bool? isUnique) : base(defaultValue, isPrivate, isRequired, isUnique)
        {
            Type = "enumeration";
            Enum = @enum;
        }
    }
}