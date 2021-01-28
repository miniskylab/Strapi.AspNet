using System;
using JetBrains.Annotations;

namespace Strapi.AspNet.Annotations
{
    [AttributeUsage(AttributeTargets.Property)]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AllowedBlocksAttribute : Attribute
    {
        public Type[] AllowedBlockTypes { get; }

        public AllowedBlocksAttribute(params Type[] allowedBlockTypes) { AllowedBlockTypes = allowedBlockTypes; }
    }
}