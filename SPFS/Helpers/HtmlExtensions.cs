using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;

namespace SPFS.Helpers
{
    public static class HtmlExtensions
    {
        public static IHtmlString DislpayFormattedFor<TModel>(this HtmlHelper<TModel> htmlHelper,Expression<Func<TModel, string>> expression)
        {
            var value = Convert.ToString(ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData).Model);
            if (string.IsNullOrEmpty(value))
            {
                return MvcHtmlString.Empty;
            }

            value = string.Join(
                "<br/>",
                value.Split(
                    new[] { Environment.NewLine },
                    StringSplitOptions.None
                ).Select(HttpUtility.HtmlEncode)
            );

            return new HtmlString(value);
        }
    }
}