using Orbital.Core;

namespace Strapi.AspNet.Engine
{
    internal interface IStrapiEditorUi : ISingletonService
    {
        void Configure();
    }
}