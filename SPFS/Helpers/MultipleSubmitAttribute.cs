using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SPFS.Helpers
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class MultipleSubmitAttribute : ActionNameSelectorAttribute
    {
        public string Name { get; set; }
        public string Argument { get; set; }

        /// <summary>
        /// Determines whether the action name is valid in the specified controller context.
        /// </summary>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="actionName">The name of the action.</param>
        /// <param name="methodInfo">Information about the action method.</param>
        /// <returns>true if the action name is valid in the specified controller context; otherwise, false.</returns>
        public override bool IsValidName(ControllerContext controllerContext, string actionName, System.Reflection.MethodInfo methodInfo)
        {
            var value = controllerContext.Controller.ValueProvider.GetValue(string.Format("{0}:{1}", Name, Argument));
            if (value != null)
            {
                controllerContext.Controller.ControllerContext.RouteData.Values[Name] = Argument;
                return true;
            }

            return false;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class MultipleSubmitDefaultAttribute : ActionNameSelectorAttribute
    {
        public string Name { get; set; }
        public string Argument { get; set; }

        /// <summary>
        /// Determines whether the action name is valid in the specified controller context.
        /// </summary>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="actionName">The name of the action.</param>
        /// <param name="methodInfo">Information about the action method.</param>
        /// <returns>true if the action name is valid in the specified controller context; otherwise, false.</returns>
        public override bool IsValidName(ControllerContext controllerContext, string actionName, System.Reflection.MethodInfo methodInfo)
        {
            if (Name == "action" && actionName != Argument)
                return false;
            foreach (var formKey in controllerContext.RequestContext.HttpContext.Request.Form.Keys)
            {
                if (formKey is string && ((string)formKey).StartsWith(Name + ":"))
                    return false;
            }
            return true;
        }
    }
}