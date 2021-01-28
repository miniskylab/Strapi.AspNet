using System;
using JetBrains.Annotations;

namespace Strapi.AspNet.Annotations
{
    [AttributeUsage(AttributeTargets.Property)]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class UidForAttribute : Attribute
    {
        public string TargetFieldName { get; }

        public UidForAttribute(string targetFieldName) { TargetFieldName = targetFieldName; }
    }
}