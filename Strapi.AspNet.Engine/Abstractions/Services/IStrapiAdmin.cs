using MiniSkyLab.Core;

namespace Strapi.AspNet.Engine
{
    public interface IStrapiAdmin : IService
    {
        string Url { get; }

        string Path { get; }

        string BaseUrl { get; }

        string ContentManagerUrl { get; }

        internal string JwtSecret { get; }

        void Authorize(IHttpClient httpClient);

        internal bool HasAdmin();

        internal void RegisterDefaultAdmin();
    }
}