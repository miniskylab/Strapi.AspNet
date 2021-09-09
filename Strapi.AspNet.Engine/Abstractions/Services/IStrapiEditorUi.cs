using MiniSkyLab.Core;

namespace Strapi.AspNet.Engine
{
    internal interface IStrapiEditorUi : ISingletonService
    {
        void Configure();
    }
}