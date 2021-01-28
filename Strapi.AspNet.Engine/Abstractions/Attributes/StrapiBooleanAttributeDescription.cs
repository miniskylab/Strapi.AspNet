namespace Strapi.AspNet.Engine
{
    internal class StrapiBooleanAttributeDescription : StrapiPrimitiveAttributeDescription<bool>
    {
        public StrapiBooleanAttributeDescription(bool defaultValue, bool? isPrivate, bool? isUnique)
            : base(defaultValue, isPrivate, false, isUnique)
        {
            Type = "boolean";
        }
    }
}