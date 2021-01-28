namespace Strapi.AspNet.Engine
{
    internal class StrapiNullableIntegerAttributeDescription : StrapiNumberAttributeDescription<int?>
    {
        public StrapiNullableIntegerAttributeDescription(int? defaultValue, int? minValue, int? maxValue, bool? isPrivate,
            bool? isRequired, bool? isUnique) : base(defaultValue, minValue, maxValue, isPrivate, isUnique)
        {
            IsRequired = isRequired;
            Type = "integer";
        }
    }
}