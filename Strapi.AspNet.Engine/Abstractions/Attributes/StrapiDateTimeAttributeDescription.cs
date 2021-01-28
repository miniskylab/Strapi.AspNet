using System;

namespace Strapi.AspNet.Engine
{
    internal class StrapiDateTimeAttributeDescription : StrapiPrimitiveAttributeDescription<DateTime>
    {
        public StrapiDateTimeAttributeDescription(DateTime defaultValue, bool? isPrivate, bool? isUnique, bool isDateOnly)
            : base(defaultValue, isPrivate, true, isUnique)
        {
            Type = isDateOnly ? "date" : "datetime";
        }
    }
}