using Newtonsoft.Json;

namespace Strapi.AspNet.Engine
{
    internal class StrapiUidAttributeDescription : StrapiStringAttributeDescription
    {
        [JsonProperty("targetField")]
        public string TargetField { get; private set; }

        public StrapiUidAttributeDescription(string defaultValue, string targetField, int? minLength, int? maxLength, bool? isPrivate,
            bool? isRequired) : base(defaultValue, "uid", minLength, maxLength, isPrivate, isRequired, null)
        {
            TargetField = targetField;
        }
    }
}