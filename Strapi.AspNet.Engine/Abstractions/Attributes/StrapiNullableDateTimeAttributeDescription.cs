using System;

namespace Strapi.AspNet.Engine
{
    internal class StrapiNullableDateTimeAttributeDescription : StrapiPrimitiveAttributeDescription<DateTime?>
    {
        public StrapiNullableDateTimeAttributeDescription(DateTime? defaultValue, bool? isPrivate, bool? isRequired, bool? isUnique,
            bool isDateOnly) : base(defaultValue, isPrivate, isRequired, isUnique)
        {
            Type = isDateOnly ? "date" : "datetime";
        }
    }
}