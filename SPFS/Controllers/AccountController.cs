using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using SPFS.Models;
using SPFS.Helpers;
using System.Web.Security;
using System.Security.Principal;
using System.Threading;

namespace SPFS.Controllers
{
    [AllowAnonymous]
    public class AccountController : BaseController
    {
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

     
       

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model,string returnUrl)
        {
            if (returnUrl != null && (returnUrl.ToLower().Contains("logoff") || returnUrl.ToLower().Contains("exception")))
                returnUrl = null;

            if (ModelState.IsValid)
            {
                bool checkUserWithAd = false;
                checkUserWithAd = true;//Membership.ValidateUser(model.UserName, model.Password)
                if (checkUserWithAd)
                {
                    Utilities utils = new Utilities();

                    String role = utils.GetRolesForCurrentUser(model.UserName);
                    GenericPrincipal principal = new GenericPrincipal(User.Identity, new string[] {role });
                    Thread.CurrentPrincipal = principal;

                    System.Web.HttpContext.Current.User = principal;


                    Logger.Log(model.UserName + " Logged in", Logging.LoggingLevel.Info);

                    FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);

                    if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\") && !returnUrl.Contains("Account/Login"))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        //return RedirectToAction("Index", "Home");
                        return RedirectToAction("LogOff", "Account");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "The user name or password provided is incorrect");
                }
            }

            // if we got this far, something failed, redisplay form
            return View(model);
        }
 

  
 
      
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult LogOff()
        {
            Utilities utils = new Utilities();
            Logger.Log(utils.GetCurrentUser().UserName + " Logged out", Logging.LoggingLevel.Info);

            ViewBag.Message = utils.GetCurrentUser().First_Name + " " + utils.GetCurrentUser().Last_Name;

            FormsAuthentication.SignOut();
            Response.Cookies[FormsAuthentication.FormsCookieName].Expires = DateTime.Now.AddDays(-1);
            System.Web.HttpContext.Current.User = null;

            return View("Logoff");
            //return RedirectToAction("Index", "Home");
        }

       

        
    }
}