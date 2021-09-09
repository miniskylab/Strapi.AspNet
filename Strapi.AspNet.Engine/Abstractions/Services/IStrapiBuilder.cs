using MiniSkyLab.Core;

namespace Strapi.AspNet.Engine
{
    internal interface IStrapiBuilder : ISingletonService
    {
        void BuildStrapiTypesFromDotnetTypes();
    }
}