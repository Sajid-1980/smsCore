using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace smsCore
{
    public static class NavigationIndicatorHelper
    {
        public static string? MakeActiveClass(this IUrlHelper urlHelper, string controller, string action)
        {
            try
            {
                string result = "active";
                string controllerName = urlHelper.ActionContext.RouteData.Values["controller"].ToString();
                string methodName = urlHelper.ActionContext.RouteData.Values["action"].ToString();
                if (string.IsNullOrEmpty(controllerName)) return null;
                if (controllerName.Equals(controller, StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrEmpty(action))
                    {
                        if (methodName.Equals(action, StringComparison.OrdinalIgnoreCase))
                        {
                            return result;
                        }
                    }
                    else
                    {
                        return result;
                    }
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static string MakeOpenClass(this IUrlHelper urlHelper, string[] controller)
        {
            try
            {
                if (controller == null || controller.Length==0)
                    return null;
                string result = "open";
                string controllerName = urlHelper.ActionContext.RouteData.Values["controller"].ToString();
                if (string.IsNullOrEmpty(controllerName)) return null;
                if (controller.Contains(controllerName))
                {
                    return result;
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }

        }
    }
}
