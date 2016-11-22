using SPFS.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace SPFS
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
        protected void Application_Error(Object sender, EventArgs e)
        {
            var raisedException = Server.GetLastError();

            //Get innermost exception
            var innerException = raisedException;

            while (raisedException != null)
            {
                innerException = raisedException;
                raisedException = raisedException.InnerException;
            }

            var _logger = new Logger();
            _logger.Log(innerException.ToString(), LoggingLevel.Error);

            var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
            Response.Redirect(urlHelper.Action("Exception", "Account"));
        }
    }
}
