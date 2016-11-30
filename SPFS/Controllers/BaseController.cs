using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SPFS.Helpers;
using SPFS.Logging;
using System.Web.Mvc.Filters;
using System.Security.Principal;
using System.Threading;
using SPFS.DAL;
using System.Text;
using OfficeOpenXml;
using System.IO;

namespace SPFS.Controllers
{
    public class BaseController : Controller
    {
        private ILogger _logger;

        public ILogger Logger
        {
            get { return _logger; }
        }


        public BaseController()
        {
            if (_logger == null)
            {
                _logger = new Logger();
            }

        }
        [AllowAnonymous]
        public ActionResult Exception()
        {
            return View();
        }

        protected override void OnAuthentication(AuthenticationContext filterContext)
        {
            ViewBag.ShowMenus = false;
            ViewBag.ShowAdmin = false;

            if (Request.IsAuthenticated)
                base.OnAuthentication(filterContext);
        }
        protected override void OnAuthorization(AuthorizationContext filterContext)
        {
            ViewBag.ShowRatings = false;
            ViewBag.ShowAdmin = false;
            if (Request.IsAuthenticated)
            {

                Utilities utils = new Utilities();
                var user = utils.GetCurrentUser();
                string controller = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
                string action = filterContext.ActionDescriptor.ActionName;
                
                if (user == null)
                {
                    filterContext.Result = new ViewResult { MasterName = "_LayoutOther", ViewName = "UnauthorizedUser", ViewData = this.ViewData };
                    //filterContext.Result = new ViewResult { ViewName = "Unauthorized", ViewData = this.ViewData };
                }
                else
                {
                    var role = utils.GetRolesForCurrentUser();

                    switch (role)
                    {
                        case Utilities.ADMIN_ROLE:
                            ViewBag.ShowRatings = true;
                            ViewBag.ShowAdmin = true;
                           
                            break;
                        case Utilities.AUTHOR_ROLE:
                            ViewBag.ShowRatings = true;
                            ViewBag.ShowAdmin = false;
                            break;
                        case Utilities.SERVICEDESK_ROLE:
                            ViewBag.ShowRatings = false;
                            ViewBag.ShowAdmin = true;
                            if (!Utilities.AlLOW_ACCESS_SERVICEDESK_CONTROLLERS.Split(',').Contains(controller))
                            {

                                var _logger = new Logger();
                                _logger.Log("Unauthorized access by " + utils.GetCurrentUser().First_Name + " " + utils.GetCurrentUser().Last_Name, LoggingLevel.Warn);

                                filterContext.Result = new ViewResult { ViewName = "Unauthorized", ViewData = this.ViewData };
                            }
                            break;
                        default:
                           
                            break;
                    }

                    if (controller.Equals("Account", StringComparison.InvariantCultureIgnoreCase) && action.Equals("LogOff", StringComparison.InvariantCultureIgnoreCase))
                    {
                        ViewBag.ShowRatings = false;
                        ViewBag.ShowAdmin = false;
                    }

                    GenericPrincipal principal = new GenericPrincipal(User.Identity, new string[] { role });
                    Thread.CurrentPrincipal = principal;
                }
            }
            base.OnAuthorization(filterContext);
        }
        public bool ExportToCSV(string objectToExport, string filename)
        {
            bool exportStatus = false;
            try
            {
                Response.Clear();
                Response.ClearHeaders();
                Response.ClearContent();
                Response.AddHeader("content-disposition", string.Format("attachment;filename=" + filename + ".csv; charset=utf-8"));
                Response.ContentType = "text/csv";
                Response.AddHeader("Pragma", "public");
                Response.ContentEncoding = System.Text.Encoding.UTF8;
                Response.BinaryWrite(Encoding.UTF8.GetPreamble());
                if (string.IsNullOrEmpty(objectToExport))
                {
                    objectToExport = " ";
                }
                Response.Write(objectToExport);
                Response.Flush();
                Response.End();
                exportStatus = true;
            }
            catch (Exception ex)
            {
                this.Logger.Log(ex.Message, Logging.LoggingLevel.Error, ex, base.User.Identity.Name, "", "", "", "ExportToCSV ", this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString());
                exportStatus = false;
            }
            return exportStatus;
        }

        
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            base.OnActionExecuting(filterContext);
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);

        }
        private bool IsAjax(ExceptionContext filterContext)
        {
            return filterContext.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

        private string GetRootException(Exception ex)
        {
            var msg = String.Empty;

            if (ex.InnerException != null)
            {
                msg = GetRootException(ex.InnerException);
            }
            else
            {
                msg = ex.Message;
            }

            return msg;
        }
        protected override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled)  //|| !filterContext.HttpContext.IsCustomErrorEnabled
            {
                return;
            }

            // if the request is AJAX return JSON else view.
            if (IsAjax(filterContext))
            {
                //Because its a exception raised after ajax invocation
                //Lets return Json
                filterContext.Result = new JsonResult()
                {
                    Data = filterContext.Exception.Message,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };


                var _logger = new Logger();
                _logger.Log(GetRootException(filterContext.Exception), LoggingLevel.Error);

                filterContext.ExceptionHandled = true;
                filterContext.HttpContext.Response.Clear();
            }
            else
            {
                base.OnException(filterContext);
            }
        }

    }
}