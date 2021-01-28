using System;
using Newtonsoft.Json;
using Strapi.AspNet.Annotations;

namespace Strapi.AspNet.Engine
{
    internal class StrapiStringAttributeDescription : StrapiPrimitiveAttributeDescription<string>
    {
        [JsonProperty("maxLength")]
        public int? MaxLength { get; private set; }

        [JsonProperty("minLength")]
        public int? MinLength { get; private set; }

        public StrapiStringAttributeDescription(
            string defaultValue,
            StrapiStringType stringType,
            int? minLength,
            int? maxLength,
            bool? isPrivate,
            bool? isRequired,
            bool? isUnique
        ) : this(
            defaultValue,
            Enum.GetName(typeof(StrapiStringType), stringType)!.ToLower(),
            minLength,
            maxLength,
            isPrivate,
            isRequired,
            isUnique
        ) { }

        protected StrapiStringAttributeDescription(
            string defaultValue,
            string stringType,
            int? minLength,
            int? maxLength,
            bool? isPrivate,
            bool? isRequired,
            bool? isUnique
        ) : base(
            defaultValue,
            isPrivate,
            isRequired,
            stringType != Enum.GetName(typeof(StrapiStringType), StrapiStringType.RichText)!.ToLower()
                ? isUnique
                : null
        )
        {
            Type = stringType;
            MinLength = minLength;
            MaxLength = maxLength;
        }
    }
}