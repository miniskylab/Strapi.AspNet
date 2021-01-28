using System;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Strapi.AspNet.Annotations
{
    [AttributeUsage(AttributeTargets.Class)]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class ContentTypeAttribute : Attribute
    {
        public string Category { get; }

        public string DisplayName { get; }

        public string Guid { get; }

        public ContentTypeAttribute(string guid, string displayName = null, string category = null)
        {
            Guid = guid;

            bool GuidStartsWithLetter() { return Regex.IsMatch(guid, "^[A-Za-z].*$"); }
            if (!GuidStartsWithLetter())
                throw new ArgumentException($"Argument [{nameof(guid)}] must start with letter.");

            DisplayName = displayName;
            Category = category;
        }
    }
}