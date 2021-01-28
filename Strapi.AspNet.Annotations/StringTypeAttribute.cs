using System;
using JetBrains.Annotations;

namespace Strapi.AspNet.Annotations
{
    [AttributeUsage(AttributeTargets.Property)]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class StringTypeAttribute : Attribute
    {
        public StrapiStringType StrapiStringType { get; }

        public StringTypeAttribute(StrapiStringType strapiStringType) { StrapiStringType = strapiStringType; }
    }
}