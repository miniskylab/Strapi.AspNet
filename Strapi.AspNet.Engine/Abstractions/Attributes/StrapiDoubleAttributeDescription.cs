namespace Strapi.AspNet.Engine
{
    internal class StrapiDoubleAttributeDescription : StrapiNumberAttributeDescription<double>
    {
        public StrapiDoubleAttributeDescription(double defaultValue, int? minValue, int? maxValue, bool? isPrivate, bool? isUnique)
            : base(defaultValue, minValue, maxValue, isPrivate, isUnique)
        {
            Type = "float";
        }
    }
}