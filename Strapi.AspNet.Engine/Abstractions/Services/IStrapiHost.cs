using Orbital.Core;

namespace Strapi.AspNet.Engine
{
    public interface IStrapiHost : ISingletonService
    {
        void Start();
    }
}