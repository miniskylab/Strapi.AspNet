using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Strapi.AspNet.Cms
{
    public static class HtmlExtensions
    {
        public static IHtmlContent PropertyFor<TModel, TResult>(this IHtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TResult>> propertySelector)
        {
            return htmlHelper.DisplayFor(propertySelector);
        }
    }
}