using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Models;
using smsCore.Data.Helpers;
using System.Runtime.InteropServices;

namespace sms.Helpers
{
    public class AuthorizationActionFilter : ActionFilterAttribute, IAuthorizationFilter
    {

        SchoolEntities db;
        ClaimHelper claimHelper;
        public AuthorizationActionFilter(SchoolEntities db,ClaimHelper _claimhelper)
        {
            this.db = db;
            claimHelper = _claimhelper;
        }
        public void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            var roles = claimHelper.GetRolesFromClaims();
            var authorized = false;
            if (roles.Contains("Admin") || roles.Contains("Developer"))
            {
                authorized = true;
            }
            else
            {
                string controller = filterContext.ActionDescriptor.RouteValues["controller"]?.ToLower()??"";
                string action = filterContext.ActionDescriptor.RouteValues["action"]?.ToLower()??"";
                string[] roleIds = db.Roles.AsNoTracking().Where(w => roles.Contains(w.Name)).Select(s => s.Id).ToArray();
                var actionExist = db.MenuActions.Where(w => w.Controller.ToLower() == controller & w.Name.ToLower() == action).Any();
                if (actionExist)
                    authorized = db.Privlidges.AsNoTracking().Where(w => roleIds.Contains(w.RoleId) & w.Action.Controller.ToLower() == controller & w.Action.Name.ToLower() == action).Any();
                else authorized = true;
            }

            if (!authorized)
            {
                //if (filterContext.HttpContext.Request.IsAjaxRequest())
                //{
                //    filterContext.HttpContext.Response.StatusCode = 400;
                //    filterContext.Result = new JsonResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, ContentType = "application/json", Data = new { status = false, message = "Unauthorized resource. You are not authorized to perform this action." } };
                //}
                //else
                //filterContext.Result =
                //new Result("Unauthorized resource. You are not authorized to perform this action.");
                filterContext.Result = new JsonResult(new { error = "You do not have permission to access this resource." })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }
        }

    }
}