using Microsoft.AspNetCore.Mvc;
using Strapi.AspNet.DataModel;

namespace Strapi.AspNet.Cms
{
    public abstract class BlockController<T> : Controller where T : BlockData
    {
        [NonAction]
        public abstract ViewResult Index(T currentBlock);
    }
}