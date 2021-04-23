using Newtonsoft.Json;
using Strapi.AspNet.Annotations;

namespace Strapi.AspNet.Engine
{
    internal class StrapiMediaAttributeDescription : StrapiPrimitiveAttributeDescription
    {
        [JsonProperty("allowedTypes")]
        public StrapiMediaType[] AllowedTypes { get; private set; }

        [JsonProperty("multiple")]
        public bool? IsMultiple { get; private set; }

        public StrapiMediaAttributeDescription(StrapiMediaType[] allowedTypes, bool? isMultiple, bool? isPrivate, bool? isRequired,
            bool? isUnique) : base(isPrivate, isRequired, isUnique)
        {
            Type = "media";
            IsMultiple = isMultiple;
            AllowedTypes = allowedTypes;
        }

        public override StrapiAttributeDescription ToStrapiAttributeModel()
        {
            return IsMultiple.HasValue && IsMultiple.Value
                ? new StrapiMultipleMediaAttributeModel(AllowedTypes, IsPrivate, IsRequired, IsUnique)
                : new StrapiSingleMediaAttributeModel(AllowedTypes, IsPrivate, IsRequired, IsUnique);
        }

        sealed class StrapiMultipleMediaAttributeModel : StrapiMediaAttributeModel
        {
            [JsonProperty("collection")]
            public string Collection { get; private set; }

            public StrapiMultipleMediaAttributeModel(StrapiMediaType[] allowedTypes, bool? isPrivate, bool? isRequired, bool? isUnique)
                : base(allowedTypes, isPrivate, isRequired, isUnique)
            {
                Collection = "file";
            }
        }

        sealed class StrapiSingleMediaAttributeModel : StrapiMediaAttributeModel
        {
            [JsonProperty("model")]
            public string Model { get; private set; }

            public StrapiSingleMediaAttributeModel(StrapiMediaType[] allowedTypes, bool? isPrivate, bool? isRequired, bool? isUnique)
                : base(allowedTypes, isPrivate, isRequired, isUnique)
            {
                Model = "file";
            }
        }

        abstract class StrapiMediaAttributeModel : StrapiMediaAttributeDescription
        {
            [JsonProperty("via")]
            public string Via { get; private set; }

            [JsonProperty("plugin")]
            public string Plugin { get; private set; }

            protected StrapiMediaAttributeModel(StrapiMediaType[] allowedTypes, bool? isPrivate, bool? isRequired, bool? isUnique)
                : base(allowedTypes, null, isPrivate, isRequired, isUnique)
            {
                Type = null;
                IsMultiple = null;

                Via = "related";
                Plugin = "upload";
            }
        }
    }
}