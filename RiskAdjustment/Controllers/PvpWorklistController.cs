using RiskAdjustment.Data.Models.HccWorkflow;
using RiskAdjustment.Data.Models.PvpWorkflow;
using RiskAdjustment.Data.Models.RefTables;
using RiskAdjustment.Data.ViewModels.PVP;
using RiskAdjustment.Logic.Global;
using RiskAdjustment.Logic.Pvp;
using RiskAdjustment.Logic.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace RiskAdjustment.Controllers
{
    public class PvpWorklistController : Controller
    {
        private Lazy<IPvpLogic> _pvpLogic;
        private Lazy<IGlobalUserLogic> _userLogic;
        private Lazy<IICD10Logic> _icd10logic;

        public PvpWorklistController(Lazy<IGlobalUserLogic> userLogic, Lazy<IICD10Logic> icd10Logic,
            Lazy<IPvpLogic> pvpLogic)
        {
            this._pvpLogic = pvpLogic;
            this._userLogic = userLogic;
            this._icd10logic = icd10Logic;
        }

        public ActionResult Index()
        {
            return RedirectToAction("PvpSelect");
        }

        public ActionResult Menu()
        {
            return View();
        }

        public ActionResult PvpSelect()
        {
            PvpMenuViewModel model = PopulatePvpMenuViewModel();
            return View(model);
        }


        public ActionResult GetContractCountsForDate()
        {
            string date = Request.Params["date"];
            PvpMenuViewModel model = PopulatePvpMenuViewModel();
            model.ContractCounts = this._pvpLogic.Value.GetContractCounts(date);
            model.CountsDate = DateTime.Parse(date);
            return PartialView("partials/_contractCounts", model);
        }

        public async Task<ActionResult> Main()
        {
            string contract = Request.Params["Contract"] ?? TempData["Contract"].ToString();
            string fileDate = Request.Params["FileDate"] ?? TempData["FileDate"].ToString();
            string email = $"{HttpContext.Session["Username"].ToString()}{"@summithealthcare.com"}";

            PvpWorklistViewModel model = new PvpWorklistViewModel
            {
                FileDate = fileDate,
                Contract = contract,
                WorklistEntries = await this._pvpLogic.Value.GetUnlockedWorklistEntriesForDateAsync(fileDate, contract),
                CurrentUser = this._userLogic.Value.GetUserByEmail(email)
            };

            TempData["FileDate"] = fileDate;
            TempData["Contract"] = contract;

            TempData.Keep();

            //model.WorklistEntries = model.WorklistEntries.Take(10).ToList();
            return View(model);
        }

        public async Task<ActionResult> MainCallBack()
        {
            string contract = Request.Params["Contract"] ?? TempData["Contract"].ToString();
            string fileDate = Request.Params["FileDate"] ?? TempData["FileDate"].ToString();
            string email = $"{HttpContext.Session["Username"].ToString()}{"@summithealthcare.com"}";

            PvpWorklistViewModel model = new PvpWorklistViewModel
            {
                FileDate = fileDate,
                Contract = contract,
                WorklistEntries = await this._pvpLogic.Value.GetUnlockedWorklistEntriesForDateAsync(fileDate, contract),
                CurrentUser = this._userLogic.Value.GetUserByEmail(email)
            };
            TempData.Keep();
            return PartialView("partials/_mainWorklist", model);
            }

        public async Task<ActionResult> PvpWorkEntry(string AppointmentID)
        {
            PvpWorkEntryViewModel model = await PopulatePvpWorkEntryViewModel(AppointmentID);
            TempData["PVPWorkListGoToEntryDetailScreen"] = "True";
            return View(model);
        }

        public async Task<ActionResult> Update(PvpWorklistEntry entry)
        {
            string username = System.Web.HttpContext.Current.Session["Username"].ToString();
            TempData["Contract"] = entry.Contract.ToString();
            TempData["FileDate"] = entry.FileDate.ToString();
            var hold = Request.Params["Hold"];
            if (entry.SendToReviewsPending)
            {
                TempData["entry"] = entry;
                return RedirectToAction("UpdateReviewInProgress");
            }

            // Update entry fields to HCC fields to 4 in length.
            // HCC fields
            if (entry.HCC01 != null && entry.HCC01.Trim().Length > 4) entry.HCC01 = entry.HCC01.Trim().Substring(0, 4);
            if (entry.HCC02 != null && entry.HCC02.Trim().Length > 4) entry.HCC02 = entry.HCC02.Trim().Substring(0, 4);
            if (entry.HCC03 != null && entry.HCC03.Trim().Length > 4) entry.HCC03 = entry.HCC03.Trim().Substring(0, 4);
            if (entry.HCC04 != null && entry.HCC04.Trim().Length > 4) entry.HCC04 = entry.HCC04.Trim().Substring(0, 4);
            if (entry.HCC05 != null && entry.HCC05.Trim().Length > 4) entry.HCC05 = entry.HCC05.Trim().Substring(0, 4);
            if (entry.HCC06 != null && entry.HCC06.Trim().Length > 4) entry.HCC06 = entry.HCC06.Trim().Substring(0, 4);
            if (entry.HCC07 != null && entry.HCC07.Trim().Length > 4) entry.HCC07 = entry.HCC07.Trim().Substring(0, 4);
            if (entry.HCC08 != null && entry.HCC08.Trim().Length > 4) entry.HCC08 = entry.HCC08.Trim().Substring(0, 4);
            if (entry.HCC09 != null && entry.HCC09.Trim().Length > 4) entry.HCC09 = entry.HCC09.Trim().Substring(0, 4);
            if (entry.HCC10 != null && entry.HCC10.Trim().Length > 4) entry.HCC10 = entry.HCC10.Trim().Substring(0, 4);

            // Dismissed HCC fields
            if (entry.DismissedHCC01 != null && entry.DismissedHCC01.Trim().Length > 4) entry.DismissedHCC01 = entry.DismissedHCC01.Trim().Substring(0, 4);
            if (entry.DismissedHCC02 != null && entry.DismissedHCC02.Trim().Length > 4) entry.DismissedHCC02 = entry.DismissedHCC02.Trim().Substring(0, 4);
            if (entry.DismissedHCC03 != null && entry.DismissedHCC03.Trim().Length > 4) entry.DismissedHCC03 = entry.DismissedHCC03.Trim().Substring(0, 4);
            if (entry.DismissedHCC04 != null && entry.DismissedHCC04.Trim().Length > 4) entry.DismissedHCC04 = entry.DismissedHCC04.Trim().Substring(0, 4);
            if (entry.DismissedHCC05 != null && entry.DismissedHCC05.Trim().Length > 4) entry.DismissedHCC05 = entry.DismissedHCC05.Trim().Substring(0, 4);

            // Patient Case HCC fields
            if (entry.PatientCaseHCC01 != null && entry.PatientCaseHCC01.Trim().Length > 4) entry.PatientCaseHCC01 = entry.PatientCaseHCC01.Trim().Substring(0, 4);
            if (entry.PatientCaseHCC02 != null && entry.PatientCaseHCC02.Trim().Length > 4) entry.PatientCaseHCC02 = entry.PatientCaseHCC02.Trim().Substring(0, 4);
            if (entry.PatientCaseHCC03 != null && entry.PatientCaseHCC03.Trim().Length > 4) entry.PatientCaseHCC03 = entry.PatientCaseHCC03.Trim().Substring(0, 4);
            if (entry.PatientCaseHCC04 != null && entry.PatientCaseHCC04.Trim().Length > 4) entry.PatientCaseHCC04 = entry.PatientCaseHCC04.Trim().Substring(0, 4);
            if (entry.PatientCaseHCC05 != null && entry.PatientCaseHCC05.Trim().Length > 4) entry.PatientCaseHCC05 = entry.PatientCaseHCC05.Trim().Substring(0, 4);

            // Patient Case HCC Replace fields
            if (entry.PatientCaseHCCReplaced01 != null && entry.PatientCaseHCCReplaced01.Trim().Length > 4) entry.PatientCaseHCCReplaced01 = entry.PatientCaseHCCReplaced01.Trim().Substring(0, 4);
            if (entry.PatientCaseHCCReplaced02 != null && entry.PatientCaseHCCReplaced02.Trim().Length > 4) entry.PatientCaseHCCReplaced02 = entry.PatientCaseHCCReplaced02.Trim().Substring(0, 4);
            if (entry.PatientCaseHCCReplaced03 != null && entry.PatientCaseHCCReplaced03.Trim().Length > 4) entry.PatientCaseHCCReplaced03 = entry.PatientCaseHCCReplaced03.Trim().Substring(0, 4);
            if (entry.PatientCaseHCCReplaced04 != null && entry.PatientCaseHCCReplaced04.Trim().Length > 4) entry.PatientCaseHCCReplaced04 = entry.PatientCaseHCCReplaced04.Trim().Substring(0, 4);
            if (entry.PatientCaseHCCReplaced05 != null && entry.PatientCaseHCCReplaced05.Trim().Length > 4) entry.PatientCaseHCCReplaced05 = entry.PatientCaseHCCReplaced05.Trim().Substring(0, 4);

            // Suspect HCC fields
            if (entry.SuspectHCC01 != null && entry.SuspectHCC01.Trim().Length > 4) entry.SuspectHCC01 = entry.SuspectHCC01.Trim().Substring(0, 4);
            if (entry.SuspectHCC02 != null && entry.SuspectHCC02.Trim().Length > 4) entry.SuspectHCC02 = entry.SuspectHCC02.Trim().Substring(0, 4);
            if (entry.SuspectHCC03 != null && entry.SuspectHCC03.Trim().Length > 4) entry.SuspectHCC03 = entry.SuspectHCC03.Trim().Substring(0, 4);
            if (entry.SuspectHCC04 != null && entry.SuspectHCC04.Trim().Length > 4) entry.SuspectHCC04 = entry.SuspectHCC04.Trim().Substring(0, 4);
            if (entry.SuspectHCC05 != null && entry.SuspectHCC05.Trim().Length > 4) entry.SuspectHCC05 = entry.SuspectHCC05.Trim().Substring(0, 4);
            if (entry.SuspectHCC06 != null && entry.SuspectHCC06.Trim().Length > 4) entry.SuspectHCC06 = entry.SuspectHCC06.Trim().Substring(0, 4);
            if (entry.SuspectHCC07 != null && entry.SuspectHCC07.Trim().Length > 4) entry.SuspectHCC07 = entry.SuspectHCC07.Trim().Substring(0, 4);
            if (entry.SuspectHCC08 != null && entry.SuspectHCC08.Trim().Length > 4) entry.SuspectHCC08 = entry.SuspectHCC08.Trim().Substring(0, 4);

            // Open Gap HCC fields
            if (entry.OpenGapHCC01 != null && entry.OpenGapHCC01.Trim().Length > 4) entry.OpenGapHCC01 = entry.OpenGapHCC01.Trim().Substring(0, 4);
            if (entry.OpenGapHCC02 != null && entry.OpenGapHCC02.Trim().Length > 4) entry.OpenGapHCC02 = entry.OpenGapHCC02.Trim().Substring(0, 4);
            if (entry.OpenGapHCC03 != null && entry.OpenGapHCC03.Trim().Length > 4) entry.OpenGapHCC03 = entry.OpenGapHCC03.Trim().Substring(0, 4);
            if (entry.OpenGapHCC04 != null && entry.OpenGapHCC04.Trim().Length > 4) entry.OpenGapHCC04 = entry.OpenGapHCC04.Trim().Substring(0, 4);
            if (entry.OpenGapHCC05 != null && entry.OpenGapHCC05.Trim().Length > 4) entry.OpenGapHCC05 = entry.OpenGapHCC05.Trim().Substring(0, 4);
            if (entry.OpenGapHCC06 != null && entry.OpenGapHCC06.Trim().Length > 4) entry.OpenGapHCC06 = entry.OpenGapHCC06.Trim().Substring(0, 4);
            if (entry.OpenGapHCC07 != null && entry.OpenGapHCC07.Trim().Length > 4) entry.OpenGapHCC07 = entry.OpenGapHCC07.Trim().Substring(0, 4);
            if (entry.OpenGapHCC08 != null && entry.OpenGapHCC08.Trim().Length > 4) entry.OpenGapHCC08 = entry.OpenGapHCC08.Trim().Substring(0, 4);
            if (entry.OpenGapHCC09 != null && entry.OpenGapHCC09.Trim().Length > 4) entry.OpenGapHCC09 = entry.OpenGapHCC09.Trim().Substring(0, 4);
            if (entry.OpenGapHCC10 != null && entry.OpenGapHCC10.Trim().Length > 4) entry.OpenGapHCC10 = entry.OpenGapHCC10.Trim().Substring(0, 4);


            // Update entry fields for ICID-10 Code fields to 7 in length.
            // HCC ICID-10 Code fields
            if (entry.HCC01Code != null && entry.HCC01Code.Trim().Length > 7) entry.HCC01Code = entry.HCC01Code.Trim().Substring(0, 7);
            if (entry.HCC02Code != null && entry.HCC02Code.Trim().Length > 7) entry.HCC02Code = entry.HCC02Code.Trim().Substring(0, 7);
            if (entry.HCC03Code != null && entry.HCC03Code.Trim().Length > 7) entry.HCC03Code = entry.HCC03Code.Trim().Substring(0, 7);
            if (entry.HCC04Code != null && entry.HCC04Code.Trim().Length > 7) entry.HCC04Code = entry.HCC04Code.Trim().Substring(0, 7);
            if (entry.HCC05Code != null && entry.HCC05Code.Trim().Length > 7) entry.HCC05Code = entry.HCC05Code.Trim().Substring(0, 7);
            if (entry.HCC06Code != null && entry.HCC06Code.Trim().Length > 7) entry.HCC06Code = entry.HCC06Code.Trim().Substring(0, 7);
            if (entry.HCC07Code != null && entry.HCC07Code.Trim().Length > 7) entry.HCC07Code = entry.HCC07Code.Trim().Substring(0, 7);
            if (entry.HCC08Code != null && entry.HCC08Code.Trim().Length > 7) entry.HCC08Code = entry.HCC08Code.Trim().Substring(0, 7);
            if (entry.HCC09Code != null && entry.HCC09Code.Trim().Length > 7) entry.HCC09Code = entry.HCC09Code.Trim().Substring(0, 7);
            if (entry.HCC10Code != null && entry.HCC10Code.Trim().Length > 7) entry.HCC10Code = entry.HCC10Code.Trim().Substring(0, 7);

            // Dismissed HCC ICID-10 Code fields
            if (entry.DismissedHCC01Code != null && entry.DismissedHCC01Code.Trim().Length > 7) entry.DismissedHCC01Code = entry.DismissedHCC01Code.Trim().Substring(0, 7);
            if (entry.DismissedHCC02Code != null && entry.DismissedHCC02Code.Trim().Length > 7) entry.DismissedHCC02Code = entry.DismissedHCC02Code.Trim().Substring(0, 7);
            if (entry.DismissedHCC03Code != null && entry.DismissedHCC03Code.Trim().Length > 7) entry.DismissedHCC03Code = entry.DismissedHCC03Code.Trim().Substring(0, 7);
            if (entry.DismissedHCC04Code != null && entry.DismissedHCC04Code.Trim().Length > 7) entry.DismissedHCC04Code = entry.DismissedHCC04Code.Trim().Substring(0, 7);
            if (entry.DismissedHCC05Code != null && entry.DismissedHCC05Code.Trim().Length > 7) entry.DismissedHCC05Code = entry.DismissedHCC05Code.Trim().Substring(0, 7);

            // Patient Case ICID-10 Code fields
            if (entry.PatientCaseCode01 != null && entry.PatientCaseCode01.Trim().Length > 7) entry.PatientCaseCode01 = entry.PatientCaseCode01.Trim().Substring(0, 7);
            if (entry.PatientCaseCode02 != null && entry.PatientCaseCode02.Trim().Length > 7) entry.PatientCaseCode02 = entry.PatientCaseCode02.Trim().Substring(0, 7);
            if (entry.PatientCaseCode03 != null && entry.PatientCaseCode03.Trim().Length > 7) entry.PatientCaseCode03 = entry.PatientCaseCode03.Trim().Substring(0, 7);
            if (entry.PatientCaseCode04 != null && entry.PatientCaseCode04.Trim().Length > 7) entry.PatientCaseCode04 = entry.PatientCaseCode04.Trim().Substring(0, 7);
            if (entry.PatientCaseCode05 != null && entry.PatientCaseCode05.Trim().Length > 7) entry.PatientCaseCode05 = entry.PatientCaseCode05.Trim().Substring(0, 7);

            // Suspect HCC ICID-10 Code fields
            if (entry.SuspectHCC01Code != null && entry.SuspectHCC01Code.Trim().Length > 7) entry.SuspectHCC01Code = entry.SuspectHCC01Code.Trim().Substring(0, 7);
            if (entry.SuspectHCC02Code != null && entry.SuspectHCC02Code.Trim().Length > 7) entry.SuspectHCC02Code = entry.SuspectHCC02Code.Trim().Substring(0, 7);
            if (entry.SuspectHCC03Code != null && entry.SuspectHCC03Code.Trim().Length > 7) entry.SuspectHCC03Code = entry.SuspectHCC03Code.Trim().Substring(0, 7);
            if (entry.SuspectHCC04Code != null && entry.SuspectHCC04Code.Trim().Length > 7) entry.SuspectHCC04Code = entry.SuspectHCC04Code.Trim().Substring(0, 7);
            if (entry.SuspectHCC05Code != null && entry.SuspectHCC05Code.Trim().Length > 7) entry.SuspectHCC05Code = entry.SuspectHCC05Code.Trim().Substring(0, 7);
            if (entry.SuspectHCC06Code != null && entry.SuspectHCC06Code.Trim().Length > 7) entry.SuspectHCC06Code = entry.SuspectHCC06Code.Trim().Substring(0, 7);
            if (entry.SuspectHCC07Code != null && entry.SuspectHCC07Code.Trim().Length > 7) entry.SuspectHCC07Code = entry.SuspectHCC07Code.Trim().Substring(0, 7);
            if (entry.SuspectHCC08Code != null && entry.SuspectHCC08Code.Trim().Length > 7) entry.SuspectHCC08Code = entry.SuspectHCC08Code.Trim().Substring(0, 7);

            // Open Gap HCC ICID-10 Code fields
            if (entry.OpenGapHCC01Code != null && entry.OpenGapHCC01Code.Trim().Length > 7) entry.OpenGapHCC01Code = entry.OpenGapHCC01Code.Trim().Substring(0, 7);
            if (entry.OpenGapHCC02Code != null && entry.OpenGapHCC02Code.Trim().Length > 7) entry.OpenGapHCC02Code = entry.OpenGapHCC02Code.Trim().Substring(0, 7);
            if (entry.OpenGapHCC03Code != null && entry.OpenGapHCC03Code.Trim().Length > 7) entry.OpenGapHCC03Code = entry.OpenGapHCC03Code.Trim().Substring(0, 7);
            if (entry.OpenGapHCC04Code != null && entry.OpenGapHCC04Code.Trim().Length > 7) entry.OpenGapHCC04Code = entry.OpenGapHCC04Code.Trim().Substring(0, 7);
            if (entry.OpenGapHCC05Code != null && entry.OpenGapHCC05Code.Trim().Length > 7) entry.OpenGapHCC05Code = entry.OpenGapHCC05Code.Trim().Substring(0, 7);
            if (entry.OpenGapHCC06Code != null && entry.OpenGapHCC06Code.Trim().Length > 7) entry.OpenGapHCC06Code = entry.OpenGapHCC06Code.Trim().Substring(0, 7);
            if (entry.OpenGapHCC07Code != null && entry.OpenGapHCC07Code.Trim().Length > 7) entry.OpenGapHCC07Code = entry.OpenGapHCC07Code.Trim().Substring(0, 7);
            if (entry.OpenGapHCC08Code != null && entry.OpenGapHCC08Code.Trim().Length > 7) entry.OpenGapHCC08Code = entry.OpenGapHCC08Code.Trim().Substring(0, 7);
            if (entry.OpenGapHCC09Code != null && entry.OpenGapHCC09Code.Trim().Length > 7) entry.OpenGapHCC09Code = entry.OpenGapHCC09Code.Trim().Substring(0, 7);
            if (entry.OpenGapHCC10Code != null && entry.OpenGapHCC10Code.Trim().Length > 7) entry.OpenGapHCC10Code = entry.OpenGapHCC10Code.Trim().Substring(0, 7);

            if (await this._pvpLogic.Value.UpdateAsync(entry, username) >= 0)
            {
                if(hold == "I")
                    this._pvpLogic.Value.ClearUserLockForAppointmentAsync(entry.LockId, username);
                TempData["Success"] = $"Entry for Appointment Number {entry.AppointmentID} Updated Successfully";
            }
            else
            {
                TempData["Failure"] = "There was a problem saving the entry.  Please try again.";
            }
            if (TempData["PVPWhichListLastVisited"] != null)
            {
                switch (TempData["PVPWhichListLastVisited"])
                {
                    case "Main":
                        return RedirectToAction("Main");
                    case "ReviewsInProgress":
                        return RedirectToAction("ReviewsInProgress");
                    case "Completed90Days":
                        return RedirectToAction("Completed90Days");
                }
            }
            
            return RedirectToAction("Main");
        }

        public ActionResult ICD10ComboBoxPartial()
        {
            return PartialView(ICD10Global.Entries);
        }

        //Get the list of reviews in progress
        public async Task<ActionResult> ReviewsInProgress()
        {
            ViewData["Title"] = "PVP Reviews in Progress";
            string email = $"{HttpContext.Session["Username"].ToString()}{"@summithealthcare.com"}";
            PvpReviewsInProgressViewModel model = new PvpReviewsInProgressViewModel();
            model.WorklistEntries = FilterEntries(await this._pvpLogic.Value.GetPvpReviewsInProgressAsync());
            model.CurrentUser = this._userLogic.Value.GetUserByEmail(email);
            return View(model);
        }

        public async Task<ActionResult> ReviewInProgress(string appointmentId)
        {
            PvpWorkEntryViewModel model = await PopulatePvpWorkEntryViewModel(appointmentId);
            string email = $"{HttpContext.Session["Username"].ToString()}{"@summithealthcare.com"}";
            model.CurrentUser = this._userLogic.Value.GetUserByEmail(email);
            return View(model);
        }

        public async Task<ActionResult> ReviewsInProgressCallback()
        {
            User u = this._userLogic.Value.GetUserByEmail($"{HttpContext.Session["Username"].ToString()}{"@summithealthcare.com"}");
            PvpReviewsInProgressViewModel model = new PvpReviewsInProgressViewModel();
            model.WorklistEntries = FilterEntries(await this._pvpLogic.Value.GetPvpReviewsInProgressAsync());
            return PartialView("partials/_reviewsInProgressWorklist", model);
        }

        public async Task<ActionResult> UpdateReviewInProgress()
        {
            PvpWorklistEntry entry = (PvpWorklistEntry)TempData["entry"];
            string username = System.Web.HttpContext.Current.Session["Username"].ToString();
            if (entry.SendToReviewsPending)
            {
                this._pvpLogic.Value.ClearAllFieldsAsync(entry, username);
                this._pvpLogic.Value.ClearUserLockForAppointmentAsync(entry.LockId, username);
                TempData["Success"] = $"Entry for Charge Number {entry.AppointmentID} was moved back to the main PVP worklist.";
                return RedirectToAction("ReviewsInProgress");
            }

            if (await this._pvpLogic.Value.UpdateAsync(entry, username) >= 0)
                TempData["Success"] = $"Entry for Charge Number {entry.AppointmentID} Updated Successfully";
            else
                TempData["Failure"] = "There was a problem saving the entry.  Please try again.";
            return RedirectToAction("ReviewsInProgress");
        }

        public async Task<ActionResult> Completed90Days()
        {
            PvpWorklistViewModel model = new PvpWorklistViewModel()
            {
                WorklistEntries = await this._pvpLogic.Value.GetCompleted90DaysAsync(),
                CurrentUser = this._userLogic.Value.GetUserByEmail($"{HttpContext.Session["Username"].ToString()}{"@summithealthcare.com"}")
            };
            return View(model);
        }

        //Callbacks

        public ActionResult PvpMainWorklistCallback()
        {
            return new EmptyResult();
        }

        public async Task<ActionResult> Completed90DaysCallback()
        {
            PvpWorklistViewModel model = new PvpWorklistViewModel()
            {
                WorklistEntries = await this._pvpLogic.Value.GetCompleted90DaysAsync(),
                CurrentUser = this._userLogic.Value.GetUserByEmail($"{HttpContext.Session["Username"].ToString()}{"@summithealthcare.com"}")
            };
            //model.WorklistEntries.ForEach(x => x.IsReadOnly = true);
            return PartialView("partials/_90DaysWorklist", model);
        }

        //Filter worklist entries on any number of criteria.  Currently filtering by username.
        private List<PvpWorklistEntry> FilterEntries(List<PvpWorklistEntry> entries)
        {            
            User user = this._userLogic.Value.GetUserByEmail($"{HttpContext.Session["Username"].ToString()}{"@summithealthcare.com"}");
            if (user.IsManagerOrAdmin())
                return entries;
            else
                return entries.Where(x => x.Reviewer == user.DisplayName).ToList();
        }

        private async Task<PvpWorkEntryViewModel> PopulatePvpWorkEntryViewModel(string appointmentID)
        {
            Task<PvpWorkEntryViewModel> resultTask = Task.Run(async () =>
            {
                string email = $"{HttpContext.Session["Username"].ToString()}{"@summithealthcare.com"}";
                PvpWorkEntryViewModel model = new PvpWorkEntryViewModel()
                {
                    ActiveUsers = this._userLogic.Value.GetActiveUsers(),
                };
                model.CurrentUser = model.ActiveUsers.FirstOrDefault(x => x.Email == email);
                model.Entry = await this._pvpLogic.Value.GetWorklistEntryandLockAsync(appointmentID, HttpContext.Session["Username"].ToString());
                model.Entry.HccGapsThisYear = await this._pvpLogic.Value.GetHccGapsThisYear(model.Entry.PatientID);
                model.Entry.HccGapsLastYear = await this._pvpLogic.Value.GetHccGapsLastYear(model.Entry.PatientID);
                model.ICD10Entries = ICD10Global.Entries;

                // n - 20240313 - INC0124213
                // The Json string was getting too long for the Json.Encode method in the cshtml,
                // so I am doing the encoding it on the server and placing it in the model.
                var serializer = new JavaScriptSerializer();
                serializer.MaxJsonLength = Int32.MaxValue;
                model.ICD10EntriesJsonString =
                   serializer.Serialize(model.ICD10Entries);

                model.Entry.StartTime = model.Entry.StartTime ?? DateTime.Now.TimeOfDay;
                model.Entry.ReviewDate = model.Entry.ReviewDate ?? DateTime.Now.Date;
                if (model.Entry.Reviewer == null)
                {

                    model.CurrentUser = this._userLogic.Value.GetUserByEmail(email);
                    model.Entry.Reviewer = model.CurrentUser.DisplayName.ToString();
                }
                model.Entry.Reviewer = model.Entry.Reviewer;
                model.Entry.PatientCaseDate = model.Entry.PatientCaseDate ?? DateTime.Now.Date;

                return model;
            });
            await Task.WhenAll(resultTask);
            return resultTask.Result;
        }

        private PvpMenuViewModel PopulatePvpMenuViewModel()
        {
            PvpMenuViewModel model = new PvpMenuViewModel()
            {
                MenuValues = this._pvpLogic.Value.GetPvpContractMenuItems(),
                ContractCounts = this._pvpLogic.Value.GetContractCounts(),
                CountsDate = DateTime.Now
            };
            return model;
        }
    }
}