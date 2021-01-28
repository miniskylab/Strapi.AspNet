using Orbital.Core;
using Strapi.AspNet.DataModel;

namespace Strapi.AspNet.Cms
{
    public interface IContentRepository : ISingletonService
    {
        PageData GetPageData(string nameInUrl);
    }
}