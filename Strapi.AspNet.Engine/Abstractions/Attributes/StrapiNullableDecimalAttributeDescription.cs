namespace Strapi.AspNet.Engine
{
    internal class StrapiNullableDecimalAttributeDescription : StrapiNumberAttributeDescription<decimal?>
    {
        public StrapiNullableDecimalAttributeDescription(decimal? defaultValue, int? minValue, int? maxValue, bool? isPrivate,
            bool? isRequired, bool? isUnique) : base(defaultValue, minValue, maxValue, isPrivate, isUnique)
        {
            IsRequired = isRequired;
            Type = "decimal";
        }
    }
}