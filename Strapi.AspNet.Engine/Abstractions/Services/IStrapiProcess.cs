using Orbital.Core;

namespace Strapi.AspNet.Engine
{
    internal interface IStrapiProcess : ISingletonService
    {
        void MarkAsInitializedSuccessfully();

        void Start();

        void Restart();
    }
}