using Microsoft.AspNetCore.Mvc;
using Strapi.AspNet.DataModel;

namespace Strapi.AspNet.Cms
{
    public abstract class PageController<T> : Controller where T : PageData
    {
        [NonAction]
        public abstract ViewResult Index(T currentPage);
    }
}