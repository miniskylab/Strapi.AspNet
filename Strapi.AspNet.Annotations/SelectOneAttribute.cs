using System;
using JetBrains.Annotations;

namespace Strapi.AspNet.Annotations
{
    [AttributeUsage(AttributeTargets.Property)]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class SelectOneAttribute : Attribute
    {
        public Type SelectionFactory { get; }

        public SelectOneAttribute(Type selectionFactory) { SelectionFactory = selectionFactory; }
    }
}