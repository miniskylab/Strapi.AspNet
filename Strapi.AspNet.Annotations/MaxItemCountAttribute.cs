using System;
using JetBrains.Annotations;

namespace Strapi.AspNet.Annotations
{
    [AttributeUsage(AttributeTargets.Property)]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class MaxItemCountAttribute : Attribute
    {
        public int MaxItemCount { get; }

        public MaxItemCountAttribute(int maxItemCount) { MaxItemCount = maxItemCount; }
    }
}