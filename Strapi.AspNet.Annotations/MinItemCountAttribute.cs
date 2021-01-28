using System;
using JetBrains.Annotations;

namespace Strapi.AspNet.Annotations
{
    [AttributeUsage(AttributeTargets.Property)]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class MinItemCountAttribute : Attribute
    {
        public int MinItemCount { get; }

        public MinItemCountAttribute(int minItemCount) { MinItemCount = minItemCount; }
    }
}