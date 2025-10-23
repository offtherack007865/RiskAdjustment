using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using RiskAdjustment.Data.Models.RefTables;
using RiskAdjustment.Logic.Global;
using Unity;
using AuthorizeAttribute = System.Web.Mvc.AuthorizeAttribute;

namespace RiskAdjustment.Attributes
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class AuthenticateAttribute : AuthorizeAttribute
    {
        private string _username;
        private IGlobalUserLogic _logic;

        public AuthenticateAttribute()
        {
            this._logic = UnityConfig.Container.Resolve<IGlobalUserLogic>();
        }
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (HttpContext.Current.Session["Username"] == null)
            {
                this._username = HttpContext.Current.User.Identity.Name.Replace(@"SUMMIT_NT\", "");
                HttpContext.Current.Session["Username"] = this._username;
                IGlobalUserLogic _logic = UnityConfig.Container.Resolve<IGlobalUserLogic>();

            }

            User u = _logic.GetUserByEmail($"{HttpContext.Current.Session["Username"].ToString()}@summithealthcare.com");
            if (u == null)
                return false;
            HttpContext.Current.Session["AccessLevel"] = u.Role;


            if (this._logic.IsValidUser(this._username))
                return true;
            else
                return false;
        }


        protected override void HandleUnauthorizedRequest(AuthorizationContext context)
        {
            context.Result = new RedirectToRouteResult(
                new RouteValueDictionary()
                {
                    { "action", "Unauthorized" },
                    {"controller", "Home" }
                });
        }

    }
}