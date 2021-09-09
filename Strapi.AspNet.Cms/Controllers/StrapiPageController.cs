using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using MiniSkyLab.Core;
using Strapi.AspNet.DataModel;

namespace Strapi.AspNet.Cms
{
    public class StrapiPageController : Controller
    {
        public StrapiPageController(IAssemblyScanner assemblyScanner, IContentRepository contentRepository,
            IServiceProvider serviceProvider)
        {
            _assemblyScanner = assemblyScanner;
            _serviceProvider = serviceProvider;
            _contentRepository = contentRepository;
        }

        [HttpGet("{*url}", Order = -999999)]
        public IActionResult Get()
        {
            var pageData = _contentRepository.GetPageData(HttpContext.Request.Path.Value.TrimStart('/'));
            if (pageData == null)
            {
                return NotFound();
            }

            var pageControllerType = _assemblyScanner.Types.Where(x =>
                x.IsClass &&
                !x.IsNested &&
                !x.IsAbstract &&
                x.IsSubclassOfRawGeneric(typeof(PageController<>))
            ).SingleOrDefault(x =>
            {
                do
                {
                    x = x.BaseType;
                    while (x != null && !x.IsGenericType)
                        x = x.BaseType;
                } while (x != null && x.GetGenericTypeDefinition() != typeof(PageController<>));

                return x?.GetGenericArguments().SingleOrDefault() == pageData.GetType();
            });

            if (pageControllerType == null)
            {
                return View($"/Views/Shared/Pages/{pageData.GetType().Name}.cshtml", pageData);
            }

            var pageControllerObject = _serviceProvider.GetRequiredService(pageControllerType);
            var viewResult = pageControllerType
                    .GetMethod(nameof(PageController<PageData>.Index))?
                    .Invoke(pageControllerObject, new object[] { pageData })
                as ViewResult;

            if (!string.IsNullOrEmpty(viewResult!.ViewName))
            {
                return viewResult;
            }

            var pageControllerName = Regex.Replace(pageControllerType.Name, $"{nameof(Controller)}$", string.Empty);
            viewResult.ViewName = $"/Views/{pageControllerName}/Index.cshtml";

            return viewResult;
        }

        #region Injected Services

        readonly IServiceProvider _serviceProvider;
        readonly IAssemblyScanner _assemblyScanner;
        readonly IContentRepository _contentRepository;

        #endregion
    }
}