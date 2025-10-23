using System.Web;
using System.Web.Mvc;
using RiskAdjustment.Filters;

namespace RiskAdjustment
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            //filters.Add(new AuthorizationFilter());
        }
    }
}
