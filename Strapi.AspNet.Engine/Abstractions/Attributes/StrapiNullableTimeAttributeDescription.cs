using System;

namespace Strapi.AspNet.Engine
{
    internal class StrapiNullableTimeAttributeDescription : StrapiPrimitiveAttributeDescription<TimeSpan?>
    {
        public StrapiNullableTimeAttributeDescription(TimeSpan? defaultValue, bool? isPrivate, bool? isRequired, bool? isUnique)
            : base(defaultValue, isPrivate, isRequired, isUnique)
        {
            Type = "time";
        }
    }
}