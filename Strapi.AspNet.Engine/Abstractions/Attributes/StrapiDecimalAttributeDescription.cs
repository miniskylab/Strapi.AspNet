namespace Strapi.AspNet.Engine
{
    internal class StrapiDecimalAttributeDescription : StrapiNumberAttributeDescription<decimal>
    {
        public StrapiDecimalAttributeDescription(decimal defaultValue, int? minValue, int? maxValue, bool? isPrivate, bool? isUnique)
            : base(defaultValue, minValue, maxValue, isPrivate, isUnique)
        {
            Type = "decimal";
        }
    }
}