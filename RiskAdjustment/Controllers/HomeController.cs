using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RiskAdjustment.Attributes;

namespace RiskAdjustment.Controllers
{

    public class HomeController : Controller
    {
        [Authenticate]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Unauthorized()
        {
            return View();
        }

    
    }
}