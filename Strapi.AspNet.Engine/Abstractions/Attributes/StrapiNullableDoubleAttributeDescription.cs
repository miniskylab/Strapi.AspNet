namespace Strapi.AspNet.Engine
{
    internal class StrapiNullableDoubleAttributeDescription : StrapiNumberAttributeDescription<double?>
    {
        public StrapiNullableDoubleAttributeDescription(double? defaultValue, int? minValue, int? maxValue, bool? isPrivate,
            bool? isRequired, bool? isUnique) : base(defaultValue, minValue, maxValue, isPrivate, isUnique)
        {
            IsRequired = isRequired;
            Type = "float";
        }
    }
}