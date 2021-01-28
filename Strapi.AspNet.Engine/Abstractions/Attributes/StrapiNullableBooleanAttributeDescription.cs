namespace Strapi.AspNet.Engine
{
    internal class StrapiNullableBooleanAttributeDescription : StrapiPrimitiveAttributeDescription<bool?>
    {
        public StrapiNullableBooleanAttributeDescription(bool? defaultValue, bool? isPrivate, bool? isUnique)
            : base(defaultValue, isPrivate, false, isUnique)
        {
            Type = "boolean";
        }
    }
}