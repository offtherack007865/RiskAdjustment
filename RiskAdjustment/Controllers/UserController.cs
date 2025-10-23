using RiskAdjustment.Attributes;
using RiskAdjustment.Data.Models.RefTables;
using RiskAdjustment.Data.ViewModels;
using RiskAdjustment.Logic.Global;
using System;
using System.Web.Mvc;

namespace RiskAdjustment.Controllers
{
    [Authenticate]
    public class UserController : Controller
    {
        public Lazy<IGlobalUserLogic> _userLogic;

        public UserController(Lazy<IGlobalUserLogic> userLogic)
        {
            this._userLogic = userLogic;
        }
        // GET: HccUser
        public ActionResult Index()
        {
            UserViewModel model = PopulateViewModel();
            return View(model);
        }

        public ActionResult Update()
        {
            if (this._userLogic.Value.UpdateUser(Request.Form, System.Web.HttpContext.Current.Session["Username"].ToString()) >= 0)
            {
                TempData["Success"] = "User Saved Successfully";
            }
            else
            {
                TempData["Failure"] = "There was a problem updating the user.  Please try again.";
            }

            UserViewModel model = PopulateViewModel();
            return RedirectToAction("Index");

        }

        public ActionResult EditCallback()
        {
            string id = Request.Params["Id"].ToString();
            UserViewModel model = PopulateViewModel();
            return PartialView("_results", model);
        }

        public ActionResult AddUser()
        {
            if (this._userLogic.Value.AddUser(Request.Form, System.Web.HttpContext.Current.Session["Username"].ToString()) > 0)
            {
                TempData["Success"] = "User Added Successfully";
            }
            else
            {
                TempData["Failure"] = "There was a problem adding the user.  Please try again.";
            }

            UserViewModel model = PopulateViewModel();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult ValidateUniqueEmail()
        {
            string email = Request.Params["email"];
            bool userExists= this._userLogic.Value.IsValidUser(email);
            return Json(userExists, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetSessionUsername()
        {
            return Json(HttpContext.Session["Username"]);
        }


        private UserViewModel PopulateViewModel()
        {
            UserViewModel model = new UserViewModel()
            {
                UserList = this._userLogic.Value.GetActiveUsers(),
                UserRoles = new UserRoles()
            };
            return model;
        }
    }
}