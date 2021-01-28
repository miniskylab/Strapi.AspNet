using System.Collections.Generic;
using Orbital.Core;

namespace Strapi.AspNet.Engine
{
    internal interface IStrapiRepository : ISingletonService
    {
        IEnumerable<StrapiComponentTypeMetadata> GetStrapiComponentTypeMetadata();

        IEnumerable<StrapiContentTypeMetadata> GetStrapiContentTypeMetadata();
    }
}