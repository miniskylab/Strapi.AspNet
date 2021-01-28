namespace Strapi.AspNet.Engine
{
    internal class StrapiIntegerAttributeDescription : StrapiNumberAttributeDescription<int>
    {
        public StrapiIntegerAttributeDescription(int defaultValue, int? minValue, int? maxValue, bool? isPrivate, bool? isUnique)
            : base(defaultValue, minValue, maxValue, isPrivate, isUnique)
        {
            Type = "integer";
        }
    }
}