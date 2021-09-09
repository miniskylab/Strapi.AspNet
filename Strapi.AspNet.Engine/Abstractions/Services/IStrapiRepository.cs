using System.Collections.Generic;
using MiniSkyLab.Core;

namespace Strapi.AspNet.Engine
{
    internal interface IStrapiRepository : ISingletonService
    {
        IEnumerable<StrapiComponentTypeMetadata> GetStrapiComponentTypeMetadata();

        IEnumerable<StrapiContentTypeMetadata> GetStrapiContentTypeMetadata();
    }
}