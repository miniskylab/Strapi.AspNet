namespace Strapi.AspNet.Engine
{
    internal class StrapiBigIntegerAttributeDescription : StrapiNumberAttributeDescription<long>
    {
        public StrapiBigIntegerAttributeDescription(long defaultValue, int? minValue, int? maxValue, bool? isPrivate, bool? isUnique)
            : base(defaultValue, minValue, maxValue, isPrivate, isUnique)
        {
            Type = "biginteger";
        }
    }
}