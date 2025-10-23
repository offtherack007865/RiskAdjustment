using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RiskAdjustment.Attributes;
using RiskAdjustment.Data.ViewModels;
using RiskAdjustment.Logic.Global;

namespace RiskAdjustment.Controllers
{
    [Authenticate]
    public class SiteController : Controller
    {
        
        private Lazy<ISiteLogic> _siteLogic;

        public SiteController(Lazy<ISiteLogic> siteLogic)
        {
            this._siteLogic = siteLogic;
        }
        // GET: Site
        public ActionResult Index()
        {
            SitesViewModel model = PopulateViewModel();
            return View(model);
        }

        public ActionResult EditCallback()
        {
            string id = Request.Params["Id"].ToString();
            SitesViewModel model = PopulateViewModel();
            return PartialView("_results", model);
        }

        public ActionResult Update()
        {
            if (this._siteLogic.Value.UpdateSite(Request.Form) >= 0)
            {
                TempData["Success"] = "User Saved Successfully";
            }
            else
            {
                TempData["Failure"] = "There was a problem updating the user.  Please try again.";
            }

            SitesViewModel model = PopulateViewModel();
            return RedirectToAction("Index");
        }

        public ActionResult AddSite()
        {
            if (this._siteLogic.Value.AddSite(Request.Form) > 0)
            {
                TempData["Success"] = "Site Added Successfully";
            }
            else
            {
                TempData["Failure"] = "There was a problem adding the site.  Please try again.";
            }

            SitesViewModel model = PopulateViewModel();
            return RedirectToAction("Index");
        }

        private SitesViewModel PopulateViewModel()
        {
            SitesViewModel model = new SitesViewModel()
            {
                Sites = this._siteLogic.Value.GetAllSites()
            };
            return model;
        }


    }
}