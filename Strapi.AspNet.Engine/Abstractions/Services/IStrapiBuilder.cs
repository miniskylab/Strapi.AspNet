using Orbital.Core;

namespace Strapi.AspNet.Engine
{
    internal interface IStrapiBuilder : ISingletonService
    {
        void BuildStrapiTypesFromDotnetTypes();
    }
}