using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Dapper.FluentMap;
using RiskAdjustment.Data.Mappings;
using WorklistEntryMap = RiskAdjustment.Data.Models.HccWorkflow.WorklistEntryMap;

namespace RiskAdjustment
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            ModelBinders.Binders.DefaultBinder = new DevExpress.Web.Mvc.DevExpressEditorsBinder();
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            FluentMapper.Initialize(config =>
            {
                config.AddMap(new WorklistEntryMap());
                config.AddMap(new ICD10EntryMap());
            });

            //Instantiate the ICD-10 Global list
            RiskAdjustment.Logic.Global.ICD10Logic iCD10Logic = new Logic.Global.ICD10Logic();
            iCD10Logic.PopulateGlobalICD10List();
        }
    }
}
