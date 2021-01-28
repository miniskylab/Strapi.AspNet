using System;

namespace Strapi.AspNet.Engine
{
    internal class StrapiTimeAttributeDescription : StrapiPrimitiveAttributeDescription<TimeSpan>
    {
        public StrapiTimeAttributeDescription(TimeSpan defaultValue, bool? isPrivate, bool? isUnique)
            : base(defaultValue, isPrivate, true, isUnique)
        {
            Type = "time";
        }
    }
}