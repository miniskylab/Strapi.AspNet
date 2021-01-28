using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.DependencyInjection;
using Orbital.Core;
using Strapi.AspNet.DataModel;

namespace Strapi.AspNet.Cms
{
    public class StrapiBlockController : ViewComponent
    {
        public StrapiBlockController(IAssemblyScanner assemblyScanner, IServiceProvider serviceProvider)
        {
            _assemblyScanner = assemblyScanner;
            _serviceProvider = serviceProvider;
        }

        public Task<ViewViewComponentResult> InvokeAsync(BlockData blockData)
        {
            var blockControllerType = _assemblyScanner.Types.Where(x =>
                x.IsClass &&
                !x.IsNested &&
                !x.IsAbstract &&
                x.IsSubclassOfRawGeneric(typeof(BlockController<>))
            ).SingleOrDefault(x =>
            {
                do
                {
                    x = x.BaseType;
                    while (x != null && !x.IsGenericType)
                        x = x.BaseType;
                } while (x != null && x.GetGenericTypeDefinition() != typeof(BlockController<>));

                return x?.GetGenericArguments().SingleOrDefault() == blockData.GetType();
            });

            if (blockControllerType == null)
            {
                return Task.FromResult(View($"/Views/Shared/Blocks/{blockData.GetType().Name}.cshtml", blockData));
            }

            var blockControllerObject = _serviceProvider.GetRequiredService(blockControllerType);
            var viewResult = (ViewResult) blockControllerType.GetMethod(nameof(BlockController<BlockData>.Index))!
                .Invoke(blockControllerObject, new object[] { blockData });

            if (!string.IsNullOrEmpty(viewResult.ViewName))
            {
                return Task.FromResult(View(viewResult.ViewName, viewResult.Model));
            }

            var blockControllerName = Regex.Replace(blockControllerType.Name, $"{nameof(Controller)}$", string.Empty);
            viewResult.ViewName = $"/Views/{blockControllerName}/Index.cshtml";

            return Task.FromResult(View(viewResult.ViewName, viewResult.Model));
        }

        #region Injected Services

        readonly IServiceProvider _serviceProvider;
        readonly IAssemblyScanner _assemblyScanner;

        #endregion
    }
}