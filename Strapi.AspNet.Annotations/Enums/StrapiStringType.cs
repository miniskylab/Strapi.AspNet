using System;
using JetBrains.Annotations;

namespace Strapi.AspNet.Annotations
{
    [Flags]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public enum StrapiStringType
    {
        String = 0,
        Text = 1 << 0,
        Email = 1 << 1,
        RichText = 1 << 2,
        Password = 1 << 3
    }
}