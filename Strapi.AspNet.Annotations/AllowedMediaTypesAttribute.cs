using System;
using JetBrains.Annotations;

namespace Strapi.AspNet.Annotations
{
    [AttributeUsage(AttributeTargets.Property)]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AllowedMediaTypesAttribute : Attribute
    {
        public StrapiMediaType[] AllowedMediaTypes { get; }

        public AllowedMediaTypesAttribute(params StrapiMediaType[] allowedMediaTypes) { AllowedMediaTypes = allowedMediaTypes; }
    }
}