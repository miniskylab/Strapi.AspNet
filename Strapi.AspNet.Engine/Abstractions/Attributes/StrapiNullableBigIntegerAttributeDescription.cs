namespace Strapi.AspNet.Engine
{
    internal class StrapiNullableBigIntegerAttributeDescription : StrapiNumberAttributeDescription<long?>
    {
        public StrapiNullableBigIntegerAttributeDescription(long? defaultValue, int? minValue, int? maxValue, bool? isPrivate,
            bool? isRequired, bool? isUnique) : base(defaultValue, minValue, maxValue, isPrivate, isUnique)
        {
            Type = "biginteger";
            IsRequired = isRequired;
        }
    }
}