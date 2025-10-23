using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using DevExpress.Web.Internal;
using RiskAdjustment.Logic.Global;
using Unity;

namespace RiskAdjustment.Filters
{
    public class AuthorizationFilter 
    {
        public Task<HttpResponseMessage> ExecuteAuthorizationFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken,
            Func<Task<HttpResponseMessage>> continuation)
        {
            GlobalUserLogic globalLogic = new GlobalUserLogic();
            if (HttpContext.Current.Session["Username"] == null)
            {
                HttpContext.Current.Session["Username"] = HttpContext.Current.User.Identity.Name.Replace(@"SUMMIT_NT\", "");
            }

            if (!globalLogic.IsValidUser(HttpContext.Current.Session["Username"].ToString()))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Forbidden));
            }
            else
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }
        }

        public bool AllowMultiple
        {
            get
            {
                return false;
            }
        }
    }
}