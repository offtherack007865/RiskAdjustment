using RiskAdjustment.Data.Models;
using RiskAdjustment.Data.Models.HccWorkflow;
using RiskAdjustment.Data.Models.RefTables;
using RiskAdjustment.Data.ViewModels.HCC;
using RiskAdjustment.Logic.Global;
using RiskAdjustment.Logic.Hcc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using RiskAdjustment.Attributes;
using RiskAdjustment.Logic.Utilities;
using System.Web.Script.Serialization;

namespace RiskAdjustment.Controllers
{
    
    [Authenticate]
    public class HccWorklistController : Controller
    {
        private Lazy<IHccLogic> _hccLogic;
        private Lazy<IGlobalUserLogic> _userLogic;
        private Lazy<IICD10Logic> _icd10logic;

        public HccWorklistController(Lazy<IHccLogic> hccLogic, Lazy<IGlobalUserLogic> userLogic, Lazy<IICD10Logic> iCD10Logic)
        {
            this._hccLogic = hccLogic;
            this._userLogic = userLogic;
            this._icd10logic = iCD10Logic;
        }

        // GET: HccWorklist
        public ActionResult Index()
        {
            return RedirectToAction("Menu");
        }

        public ActionResult Menu()
        {
            HccMenuViewModel menuModel = PopulateHccMenuViewModel();
            return View();
        }

        public ActionResult HccSelect()
        {
            HccMenuViewModel menuModel = PopulateHccMenuViewModel();
            return View(menuModel);
        }

        public async Task<ActionResult> Main()
        {
            string contract = Request.Params["Contract"] ?? TempData["Contract"].ToString();
            string fileDate = Request.Params["FileDate"] ?? TempData["FileDate"].ToString();
            string email = $"{HttpContext. Session["Username"].ToString()}{"@summithealthcare.com"}";
            
            HccWorklistViewModel model = new HccWorklistViewModel()
            {
                FileDate = fileDate,
                Contract = contract
            };
            model.CurrentUser = this._userLogic.Value.GetUserByEmail(email);
            model.WorklistEntries = await this._hccLogic.Value.GetUnlockedWorklistEntriesForDateAsync(fileDate, contract);
            //model.WorklistEntries = model.WorklistEntries.Take(20).ToList();
            model.CurrentUser = this._userLogic.Value.GetUserByEmail(email);


            TempData["WorklistType"] = "workentry";

            return View(model);
        }

        public async Task<ActionResult> MainWorklistCallback()
        {
            string email = $"{HttpContext.Session["Username"].ToString()}{"@summithealthcare.com"}";
            HccWorklistViewModel model = new HccWorklistViewModel()
            {
                FileDate = Request.Params["fileDate"],
                Contract = Request.Params["contract"]
            };
            model.WorklistEntries = await this._hccLogic.Value.GetUnlockedWorklistEntriesForDateAsync(Request.Params["fileDate"], Request.Params["contract"]);
            model.WorklistEntries = model.WorklistEntries.Take(20).ToList();
            model.CurrentUser = this._userLogic.Value.GetUserByEmail(email);

            TempData["WorklistType"] = "workentry";

            return PartialView("partials/_mainWorklist", model);
        }

        public async Task<ActionResult> WorkEntry(string ChargeID)
        {
            HccWorkEntryViewModel model = await PopulateHccWorkEntryViewModel(ChargeID, false);
            string email = $"{HttpContext.Session["Username"].ToString()}{"@summithealthcare.com"}";
            model.CurrentUser = this._userLogic.Value.GetUserByEmail(email);
            return View(model);
        }

        public ActionResult ICD10ComboBoxPartial()
        {
            return PartialView(ICD10Global.Entries);
        }

        [HttpPost]
        public async Task<ActionResult> Update(WorklistEntry entry)
        {
            string username = System.Web.HttpContext.Current.Session["Username"].ToString();
            TempData["Contract"] = entry.Contract;
            TempData["FileDate"] = entry.FileDate.ToString();

            if (entry.RptCat_Hold || entry.SendToReviewsPending)
            {
                TempData["entry"] = entry;
                return RedirectToAction("UpdateReviewInProgress");
            }

            if (await this._hccLogic.Value.UpdateAsync(entry, username) >= 0)
            {
                if (!entry.Hold && !entry.IsChildEntry())
                    this._hccLogic.Value.ClearUserLockForCharge(entry.LockId, username);
                TempData["Success"] = $"Entry for Charge Number {entry.ChargeID} Updated Successfully";
            }
            else
            {
                TempData["Failure"] = "There was a problem saving the entry.  Please try again.";
            }

            string _listToWhichToReturn = "";
            if (entry.IsChildEntry())
            {
                // pwm - 20220705 - Instead of going back to the entry, go back to the source list.
                //int priorityCharge = this._hccLogic.Value.GetHighestPriorityItemForClaim(entry.ClaimID).Result.ChargeID;
                //if (entry.InPendingBucket)
                //    return RedirectToAction("ReviewInProgress", new { ChargeID = priorityCharge.ToString() });
                //else
                //    return RedirectToAction("WorkEntry", new { ChargeID = priorityCharge.ToString() });

                _listToWhichToReturn = "";
                if (TempData["WorklistType"] != null)
                {
                    _listToWhichToReturn = TempData["WorklistType"].ToString();
                }
                switch (_listToWhichToReturn)
                {
                    case "inprogress":
                        return RedirectToAction("ReviewsInProgress");
                    case "escalated":
                        return RedirectToAction("ReviewsEscalated");
                    case "pended":
                        return RedirectToAction("ReviewsOnHoldPended");
                    case "workentry":
                        return RedirectToAction("Main");
                    default:
                        return RedirectToAction("Main");
                }
            }
            else
            {
                _listToWhichToReturn = "";
                if (TempData["WorklistType"] != null)
                {
                    _listToWhichToReturn = TempData["WorklistType"].ToString();
                }
                switch (_listToWhichToReturn)
                {
                    case "inprogress":
                        return RedirectToAction("ReviewsInProgress");
                    case "escalated":
                        return RedirectToAction("ReviewsEscalated");
                    case "pended":
                        return RedirectToAction("ReviewsOnHoldPended");
                    case "workentry":
                        return RedirectToAction("Main");
                    default:
                        return RedirectToAction("Main");
                }
            }
        }

        public ActionResult GetContractCountsForDate()
        {
            string date = Request.Params["date"];
            HccMenuViewModel model = PopulateHccMenuViewModel();
            model.ContractCounts = this._hccLogic.Value.GetContractCounts(date);
            model.CountsDate = DateTime.Parse(date);
            return PartialView("partials/_contractCounts", model);
        }

        public async Task<ActionResult> Completed90DaysCallback()
        {
            HccWorklistViewModel model = new HccWorklistViewModel()
            {
                WorklistEntries = await this._hccLogic.Value.GetCompleted90DaysAsync(),
                CurrentUser = this._userLogic.Value.GetUserByEmail($"{HttpContext.Session["Username"].ToString()}{"@summithealthcare.com"}")
            };
            model.WorklistEntries.ForEach(x => x.IsReadOnly = true);
            return PartialView("partials/_90DaysWorklist", model);
        }


        #region Reviews In Progress
        public async Task<ActionResult> ReviewsInProgress()
        {
            ViewData["Title"] = "Reviews in Progress";
            string email = $"{HttpContext.Session["Username"].ToString()}{"@summithealthcare.com"}";
            HccReviewsPendingViewModel model = new HccReviewsPendingViewModel();
            model.WorklistEntries = FilterEntries(await Task.Run(() => this._hccLogic.Value.GetReviewsInProgressAsync().Result.Where(x => x.RptCat_Hold == false).ToList()));

            TempData["WorklistType"] = "inprogress";

            model.CurrentUser = this._userLogic.Value.GetUserByEmail(email);
            return View(model);
        }

        public async Task<ActionResult> ReviewsInProgressCallback()
        {
            User u = this._userLogic.Value.GetUserByEmail($"{HttpContext.Session["Username"].ToString()}{"@summithealthcare.com"}");
            HccReviewsPendingViewModel model = new HccReviewsPendingViewModel();
            model.WorklistEntries = FilterEntries(await Task.Run(() => this._hccLogic.Value.GetReviewsInProgressAsync().Result.Where(x => x.RptCat_Hold == false).ToList()));

            TempData["WorklistType"] = "inprogress";

            return PartialView("partials/_reviewsInProgressWorklist", model);
        }

        public async Task<ActionResult> ReviewInProgress(string ChargeID)
        {
            HccWorkEntryViewModel model = await PopulateHccWorkEntryViewModel(ChargeID, true);
            string email = $"{HttpContext.Session["Username"].ToString()}{"@summithealthcare.com"}";
            model.CurrentUser = this._userLogic.Value.GetUserByEmail(email);
            return View(model);
        }

        public async Task<ActionResult> UpdateReviewInProgress()
        {
            WorklistEntry entry = (WorklistEntry)TempData["entry"];
            string username = System.Web.HttpContext.Current.Session["Username"].ToString();

            string _listToWhichToReturn = "";
            if (entry.SendToReviewsPending)
            {
                this._hccLogic.Value.ClearAllFieldsAsync(entry, username);
                this._hccLogic.Value.ClearUserLockForCharge(entry.LockId, username);
                TempData["Success"] = $"Entry for Charge Number {entry.ChargeID} was moved back to Reviews Pending.";

                // pwm - 20220707 - Instead of going back to ReviewsInProgress, go to source list.
                //return RedirectToAction("ReviewsInProgress");
                _listToWhichToReturn = "";
                if (TempData["WorklistType"] != null)
                {
                    _listToWhichToReturn = TempData["WorklistType"].ToString();
                }
                switch (_listToWhichToReturn)
                {
                    case "inprogress":
                        return RedirectToAction("ReviewsInProgress");
                    case "escalated":
                        return RedirectToAction("ReviewsEscalated");
                    case "pended":
                        return RedirectToAction("ReviewsOnHoldPended");
                    case "workentry":
                        return RedirectToAction("Main");
                    default:
                        return RedirectToAction("Main");
                }
            }

            if (!entry.IsChildEntry() && !entry.RptCat_Hold)
                this._hccLogic.Value.ClearUserLockForCharge(entry.LockId, username);

            if (await this._hccLogic.Value.UpdateAsync(entry, username) >= 0)
                TempData["Success"] = $"Entry for Charge Number {entry.ChargeID} Updated Successfully";
            else
                TempData["Failure"] = "There was a problem saving the entry.  Please try again.";

            _listToWhichToReturn = "";
            if (TempData["WorklistType"] != null)
            {
                _listToWhichToReturn = TempData["WorklistType"].ToString();
            }
            switch (_listToWhichToReturn)
            {
                case "inprogress":
                    return RedirectToAction("ReviewsInProgress");
                case "escalated":
                    return RedirectToAction("ReviewsEscalated");
                case "pended":
                    return RedirectToAction("ReviewsOnHoldPended");
                case "workentry":
                    return RedirectToAction("Main");
                default:
                    return RedirectToAction("Main");
            }
        }

        #endregion

        #region Reviews In Progress - Escalated and Pending
        public async Task<ActionResult> ReviewsEscalated()
        {
            ViewData["Title"] = "Escalated Reviews on Hold (Manager Only)";
            ViewData["Action"] = "escalated";
            HccReviewsPendingViewModel model = await PopulateReviewEscalatedViewModel();

            TempData["WorklistType"] = "escalated";

            return View("ReviewsInProgress", model);
        }

        public async Task<ActionResult> ReviewsEscalatedCallback()
        {
            HccReviewsPendingViewModel model = await PopulateReviewEscalatedViewModel();

            TempData["WorklistType"] = "escalated";

            return PartialView("partials/_escalatedReviewsWorklist", model);
        }

        public async Task<ActionResult> ReviewsOnHoldPended()
        {
            ViewData["Title"] = "Pended Reviews on Hold";
            ViewData["Action"] = "pended";
            HccReviewsPendingViewModel model = await PopulateReviewsPendedViewModel();

            TempData["WorklistType"] = "pended";

            return View("ReviewsInProgress", model);
        }

        public async Task<ActionResult> ReviewsPendedCallback()
        {
            HccReviewsPendingViewModel model = await PopulateReviewsPendedViewModel();

            TempData["WorklistType"] = "pended";

            return PartialView("partials/_pendedReviewsWorklist", model);
        }

        private async Task<HccReviewsPendingViewModel> PopulateReviewEscalatedViewModel()
        {
            Task<HccReviewsPendingViewModel> modelResult = Task.Run(async () =>
            {
                HccReviewsPendingViewModel model = new HccReviewsPendingViewModel();
                model.WorklistEntries = await Task.Run(() => this._hccLogic.Value.GetReviewsInProgressAsync().Result.Where(x => x.Hold_Reason == "Escalated" && x.RptCat_Hold).ToList());
                model.CurrentUser = this._userLogic.Value.GetUserByEmail($"{HttpContext.Session["Username"].ToString()}{"@summithealthcare.com"}");
                return model;
            });
            await Task.WhenAll(modelResult);
            return modelResult.Result;
        }

        private async Task<HccReviewsPendingViewModel> PopulateReviewsPendedViewModel()
        {
            Task<HccReviewsPendingViewModel> modelResult = Task.Run(async () =>
            {
                HccReviewsPendingViewModel model = new HccReviewsPendingViewModel();
                model.WorklistEntries = await Task.Run(() => this._hccLogic.Value.GetReviewsInProgressAsync().Result.Where(x => x.Hold_Reason == "Pended" && x.RptCat_Hold).ToList());
                model.CurrentUser = this._userLogic.Value.GetUserByEmail($"{HttpContext.Session["Username"].ToString()}{"@summithealthcare.com"}");
                return model;
            });
            await Task.WhenAll(modelResult);
            return modelResult.Result;
        }
        #endregion

        #region Reports

        public async Task<ActionResult> Completed90Days()
        {
            HccWorklistViewModel model = new HccWorklistViewModel()
            {
                WorklistEntries = await this._hccLogic.Value.GetCompleted90DaysAsync(),
                CurrentUser = this._userLogic.Value.GetUserByEmail($"{HttpContext.Session["Username"].ToString()}{"@summithealthcare.com"}")
            };
            model.WorklistEntries.ForEach(x => x.IsReadOnly = true);
            return View(model);
        }

        #endregion


        public List<WorklistEntry> FilterEntries(List<WorklistEntry> entries)
        {
            foreach (WorklistEntry entry in entries)
                entry.InPendingBucket = true;
            User user = this._userLogic.Value.GetUserByEmail($"{HttpContext.Session["Username"].ToString()}{"@summithealthcare.com"}");
            if (user.IsManagerOrAdmin())
                return entries;
            else
                return entries.Where(x => x.Reviewer == user.DisplayName).ToList();
        }

        //View model population

        private HccMenuViewModel PopulateHccMenuViewModel()
        {
            HccMenuViewModel model = new HccMenuViewModel();
            model.MenuValues = this._hccLogic.Value.GetContractMenuItems();
            model.ContractCounts = this._hccLogic.Value.GetContractCounts();
            model.CountsDate = DateTime.Now;
            return model;
        }

        private async Task<HccWorkEntryViewModel> PopulateHccWorkEntryViewModel(string chargeId, bool isReviewInProgress)
        {
            Task<HccWorkEntryViewModel> resultModel = Task.Run(async () =>
            {
                HccWorkEntryViewModel model = new HccWorkEntryViewModel();
                string email = $"{HttpContext.Session["Username"].ToString()}{"@summithealthcare.com"}";
                model.ActiveUsers = this._userLogic.Value.GetActiveUsers();
                model.CurrentUser = model.ActiveUsers.FirstOrDefault(x => x.Email == email);

                if(TempData["Completed"] == null)
                {
                    model.Entry = await this._hccLogic.Value.GetWorklistEntryandLockAsync(chargeId, HttpContext.Session["Username"].ToString());
                }
                else
                {
                    model.Entry = await this._hccLogic.Value.GetWorklistEntryandLockAsyncCompleted(chargeId, HttpContext.Session["Username"].ToString());
                }

                if(model.Entry.Reviewer == null)
                {
                    
                    model.CurrentUser = this._userLogic.Value.GetUserByEmail(email);
                    model.Entry.Reviewer = model.CurrentUser.DisplayName.ToString();
                }
                model.Entry.Reviewer = model.Entry.Reviewer;
                model.Entry.ReviewDate = model.Entry.ReviewDate ?? DateTime.Now.Date;
                model.Entry.InPendingBucket = isReviewInProgress;
                model.RelatedCharges = this._hccLogic.Value.GetRelatedCptsAsync(model.Entry.ClaimID, model.Entry.ChargeID, model.Entry.PatientID, model.Entry.TransactionSrvDate, model.Entry.DepartmentBillingName).Result;
                model.PreviousNotes = await this._hccLogic.Value.GetPtPreviousWorkNotesAsync(model.Entry.ChargeID);
                model.ICD10Entries = ICD10Global.Entries;  //this._icd10logic.Value.GetEntries();


                // pwm - 20240313 - INC0124213
                // The Json string was getting too long for the Json.Encode method in the cshtml,
                // so I am doing the encoding it on the server and placing it in the model.
                var serializer = new JavaScriptSerializer();
                serializer.MaxJsonLength = Int32.MaxValue;
                model.ICD10EntriesJsonString =
                   serializer.Serialize(model.ICD10Entries);

                return model;
            });
            await Task.WhenAll(resultModel);
            return resultModel.Result;
        }
        public ActionResult PeerReview()
        {
            //Added Temp Data for when calling needing to change peer review date and contract resets temp data set. 
            TempData.Remove("FileDate");
            TempData.Remove("Contract");
            TempData.Remove("Once");


            HccMenuViewModel menuModel = PopulateCompletedViewModel();

            return View(menuModel);
        }

        [HttpPost]
        public async Task<ActionResult> PeerReviewContract()
        {
            HccWorklistViewModel model = new HccWorklistViewModel();


            if (Request.Form["Agree"] != null || Request.Form["Disagree"] != null)
            {
                if (Request.Form["Agree"] != null)
                {
                    bool agree = true;
                    WorklistEntry agreeWorklist = new WorklistEntry();
                    string claimID = Request.Params["Entry.ClaimID"];
                    string agreeNote = Request.Params["Entry.notes"];
                    agreeWorklist.ClaimID = Convert.ToInt32(claimID);
                    HccWorklistViewModel agreedModel = new HccWorklistViewModel();
                    agreedModel.WorklistEntries = await this._hccLogic.Value.selectFromDatabase(agreeWorklist);

                    agreeWorklist = agreedModel.WorklistEntries[0];

                    string sql = "";
                    this._hccLogic.Value.InsertToCompletedDatabase(agreeWorklist, agree, sql, agreeNote);







                }
                if (Request.Form["Disagree"] != null)
                {
                    bool disagree = false;
                    WorklistEntry disagreeWorklist = new WorklistEntry();
                    string disagreeNote = Request.Params["Entry.notes"];
                    string claimID = Request.Params["Entry.ClaimID"];
                    disagreeWorklist.ClaimID = Convert.ToInt32(claimID);
                    HccWorklistViewModel disagreedModel = new HccWorklistViewModel();
                    disagreedModel.WorklistEntries = await this._hccLogic.Value.selectFromDatabase(disagreeWorklist);

                    disagreeWorklist = disagreedModel.WorklistEntries[0];

                    string sql = "";
                    this._hccLogic.Value.InsertToCompletedDatabase(disagreeWorklist, disagree, sql, disagreeNote);



                }
            }


            if (TempData["Once"] == null)
            {
                string contract = Request.Params["Contract"];
                string fileDate = Request.Params["FileDate"];
                string email = $"{HttpContext.Session["Username"].ToString()}{"@summithealthcare.com"}";
                TempData["Contract"] = contract;
                TempData["FileDate"] = fileDate;
                model = new HccWorklistViewModel()
                {
                    FileDate = fileDate,
                    Contract = contract
                };
                model.WorklistEntries = await this._hccLogic.Value.PeerReviewCompleted(fileDate, contract);
                //model.WorklistEntries = model.WorklistEntries.Take(20).ToList();
                model.CurrentUser = this._userLogic.Value.GetUserByEmail(email);
                TempData["Once"] = "Once";
                TempData.Keep();
            }
            else
            {
                string contract = TempData["Contract"].ToString();
                string fileDate = TempData["FileDate"].ToString();
                string email = $"{HttpContext.Session["Username"].ToString()}{"@summithealthcare.com"}";
                model = new HccWorklistViewModel()
                {
                    FileDate = fileDate,
                    Contract = contract
                };
                model.WorklistEntries = await this._hccLogic.Value.PeerReviewCompleted(fileDate, contract);
                //model.WorklistEntries = model.WorklistEntries.Take(20).ToList();
                model.CurrentUser = this._userLogic.Value.GetUserByEmail(email);
                TempData.Keep();

            }




            return View(model);
        }

        public async Task<ActionResult> PeerReviewCallback()
        {
            HccWorklistViewModel model = new HccWorklistViewModel();



            if (TempData["Once"] == null)
            {
                string contract = Request.Params["Contract"];
                string fileDate = Request.Params["FileDate"];
                string email = $"{HttpContext.Session["Username"].ToString()}{"@summithealthcare.com"}";
                TempData["Contract"] = contract;
                TempData["FileDate"] = fileDate;
                model = new HccWorklistViewModel()
                {
                    FileDate = fileDate,
                    Contract = contract
                };
                model.WorklistEntries = await this._hccLogic.Value.PeerReviewCompleted(fileDate, contract);
                //model.WorklistEntries = model.WorklistEntries.Take(20).ToList();
                model.CurrentUser = this._userLogic.Value.GetUserByEmail(email);
                TempData["Once"] = "Once";
                TempData.Keep();
            }
            else
            {
                string contract = TempData["Contract"].ToString();
                string fileDate = TempData["FileDate"].ToString();
                string email = $"{HttpContext.Session["Username"].ToString()}{"@summithealthcare.com"}";
                model = new HccWorklistViewModel()
                {
                    FileDate = fileDate,
                    Contract = contract
                };
                model.WorklistEntries = await this._hccLogic.Value.PeerReviewCompleted(fileDate, contract);
                //model.WorklistEntries = model.WorklistEntries.Take(20).ToList();
                model.CurrentUser = this._userLogic.Value.GetUserByEmail(email);
                TempData.Keep();

            }

            return PartialView("partials/_PeerReview", model);
        }

        public async Task<ActionResult> PeerEntry(string ChargeID)
        {
            HccWorkEntryViewModel model = await PopulateHccWorkEntryViewModel(ChargeID, false);
            string email = $"{HttpContext.Session["Username"].ToString()}{"@summithealthcare.com"}";
            model.CurrentUser = this._userLogic.Value.GetUserByEmail(email);
            return View(model);
        }

        public ActionResult PeerReviewCompleted()
        {
            //Added Temp Data for when calling needing to change peer review date and contract resets temp data set. 
            TempData.Remove("FileDate");
            TempData.Remove("Contract");
            TempData.Remove("Once");


            HccMenuViewModel menuModel = PopulateCompletedViewModel();

            return View(menuModel);
        }
        public async Task<ActionResult> SelectedPeerReview()
        {
            HccWorklistViewModel model = new HccWorklistViewModel();


            if (Request.Form["Agree"] != null || Request.Form["Disagree"] != null)
            {
                if (Request.Form["Agree"] != null)
                {
                    bool agree = true;
                    WorklistEntry agreeWorklist = new WorklistEntry();
                    string claimID = Request.Params["Entry.ClaimID"];
                    string Note = Request.Params["Entry.notes"];
                    agreeWorklist.ClaimID = Convert.ToInt32(claimID);
                    HccWorklistViewModel agreedModel = new HccWorklistViewModel();
                    agreedModel.WorklistEntries = await this._hccLogic.Value.SelectCompletedReview(agreeWorklist);

                    agreeWorklist = agreedModel.WorklistEntries[0];

                    string sql = "";
                    this._hccLogic.Value.UpdateCompletedReview(agreeWorklist, agree, sql,Note);




                }
                if (Request.Form["Disagree"] != null)
                {
                    bool disagree = false;
                    WorklistEntry disagreeWorklist = new WorklistEntry();
                    string claimID = Request.Params["Entry.ClaimID"];
                    string Note = Request.Params["Entry.notes"];
                    disagreeWorklist.ClaimID = Convert.ToInt32(claimID);
                    HccWorklistViewModel disagreedModel = new HccWorklistViewModel();
                    disagreedModel.WorklistEntries = await this._hccLogic.Value.SelectCompletedReview(disagreeWorklist);

                    disagreeWorklist = disagreedModel.WorklistEntries[0];

                    string sql = "";
                    this._hccLogic.Value.UpdateCompletedReview(disagreeWorklist, disagree, sql,Note);



                }
            }


            if (TempData["Once"] == null)
            {
                string contract = Request.Params["Contract"];
                string fileDate = Request.Params["FileDate"];
                string email = $"{HttpContext.Session["Username"].ToString()}{"@summithealthcare.com"}";
                TempData["Contract"] = contract;
                TempData["FileDate"] = fileDate;
                model = new HccWorklistViewModel()
                {
                    FileDate = fileDate,
                    Contract = contract
                };
                model.WorklistEntries = await this._hccLogic.Value.PeerReviewManagers(fileDate, contract);
                model.WorklistEntries = model.WorklistEntries.Take(20).ToList();
                model.CurrentUser = this._userLogic.Value.GetUserByEmail(email);
                TempData["Once"] = "Once";

                TempData.Keep();
            }
            else
            {
                string contract = TempData["Contract"].ToString();
                string fileDate = TempData["FileDate"].ToString();
                string email = $"{HttpContext.Session["Username"].ToString()}{"@summithealthcare.com"}";
                model = new HccWorklistViewModel()
                {
                    FileDate = fileDate,
                    Contract = contract
                };
                model.WorklistEntries = await this._hccLogic.Value.PeerReviewManagers(fileDate, contract);
                model.WorklistEntries = model.WorklistEntries.Take(20).ToList();
                model.CurrentUser = this._userLogic.Value.GetUserByEmail(email);


                TempData.Keep();

            }


            //the next lines are here to set up a way to set up a new field wanted for the managers to see if the claim was agreed or disagreed on

            if (model.WorklistEntries.Count > 0)
            {
                int i = 0;
                while (i < model.WorklistEntries.Count)
                {
                    WorklistEntry decisionModel = new WorklistEntry();
                    decisionModel.ClaimID = model.WorklistEntries[i].ClaimID;
                    decisionModel = this._hccLogic.Value.DecisionSetUp(decisionModel);
                    if (decisionModel.agree != false)
                    {
                        model.WorklistEntries[i].decision = "Agreed";
                    }
                    else if (decisionModel.disagree != false)
                    {
                        model.WorklistEntries[i].decision = "Disagree";
                    }

                    i++;
                }

            }



            return View(model);
        }

        public async Task<ActionResult> ManagersCompletedViewCalback()
        {
            HccWorklistViewModel model = new HccWorklistViewModel();


            if (TempData["Once"] == null)
            {
                string contract = Request.Params["Contract"];
                string fileDate = Request.Params["FileDate"];
                string email = $"{HttpContext.Session["Username"].ToString()}{"@summithealthcare.com"}";
                TempData["Contract"] = contract;
                TempData["FileDate"] = fileDate;
                model = new HccWorklistViewModel()
                {
                    FileDate = fileDate,
                    Contract = contract
                };
                model.WorklistEntries = await this._hccLogic.Value.PeerReviewManagers(fileDate, contract);
                model.WorklistEntries = model.WorklistEntries.Take(20).ToList();
                model.CurrentUser = this._userLogic.Value.GetUserByEmail(email);
                TempData["Once"] = "Once";

                TempData.Keep();
            }
            else
            {
                string contract = TempData["Contract"].ToString();
                string fileDate = TempData["FileDate"].ToString();
                string email = $"{HttpContext.Session["Username"].ToString()}{"@summithealthcare.com"}";
                model = new HccWorklistViewModel()
                {
                    FileDate = fileDate,
                    Contract = contract
                };
                model.WorklistEntries = await this._hccLogic.Value.PeerReviewManagers(fileDate, contract);
                model.WorklistEntries = model.WorklistEntries.Take(20).ToList();
                model.CurrentUser = this._userLogic.Value.GetUserByEmail(email);


                TempData.Keep();

            }


            //the next lines are here to set up a way to set up a new field wanted for the managers to see if the claim was agreed or disagreed on

            if (model.WorklistEntries.Count > 0)
            {
                int i = 0;
                while (i < model.WorklistEntries.Count)
                {
                    WorklistEntry decisionModel = new WorklistEntry();
                    decisionModel.ClaimID = model.WorklistEntries[i].ClaimID;
                    decisionModel = this._hccLogic.Value.DecisionSetUp(decisionModel);
                    if (decisionModel.agree != false)
                    {
                        model.WorklistEntries[i].decision = "Agreed";
                    }
                    else if (decisionModel.disagree != false)
                    {
                        model.WorklistEntries[i].decision = "Disagree";
                    }

                    i++;
                }

            }

            return PartialView("partials/_CompletedPeerReview", model);
        }

        public async Task<ActionResult> ManagersCompletedView(string ChargeID)
        {
            TempData["Completed"] = "Completed";
            TempData.Keep();
            HccWorkEntryViewModel model = await PopulateHccWorkEntryViewModel(ChargeID, false);
            string email = $"{HttpContext.Session["Username"].ToString()}{"@summithealthcare.com"}";
            model.CurrentUser = this._userLogic.Value.GetUserByEmail(email);
            HccWorklistViewModel boolModel = new HccWorklistViewModel();
            boolModel.WorklistEntries = await this._hccLogic.Value.SelectBoolsFromDatabase(model);
            if (boolModel.WorklistEntries[0].agree == true)
            {
                model.Entry.decision = "Agreed";
                model.Entry.notes = null;

            }
            else if (boolModel.WorklistEntries[0].disagree == true)
            {
                model.Entry.decision = "Disagreed";
                model.Entry.notes = boolModel.WorklistEntries[0].notes;
            }

            TempData.Remove("Completed");

            return View(model);
        }

        private HccMenuViewModel PopulateCompletedViewModel()
        {
            HccMenuViewModel model = new HccMenuViewModel();
            model.MenuValues = this._hccLogic.Value.GetContractMenuItemsCompleted();
            model.ContractCounts = this._hccLogic.Value.GetContractCountsConpleted();
            model.CountsDate = DateTime.Now;
            return model;
        }




        public async Task<ActionResult> AllPeerReviews()
        {
            HccWorklistViewModel model = new HccWorklistViewModel()
            {
                WorklistEntries = await this._hccLogic.Value.SelectAllPeerReviews(),
                CurrentUser = this._userLogic.Value.GetUserByEmail($"{HttpContext.Session["Username"].ToString()}{"@summithealthcare.com"}")
            };
            model.WorklistEntries.ForEach(x => x.IsReadOnly = true);

            if (model.WorklistEntries.Count > 0)
            {
                int i = 0;
                while (i < model.WorklistEntries.Count)
                {
                    WorklistEntry decisionModel = new WorklistEntry();
                    decisionModel.ClaimID = model.WorklistEntries[i].ClaimID;
                    decisionModel = this._hccLogic.Value.DecisionSetUp(decisionModel);
                    if (decisionModel.agree != false)
                    {
                        model.WorklistEntries[i].decision = "Agreed";
                    }
                    else if (decisionModel.disagree != false)
                    {
                        model.WorklistEntries[i].decision = "Disagree";
                    }

                    i++;
                }

               
            }
            return View(model);
        }

        public async Task<ActionResult> AllPeerReviewsCallback()
        {
            HccWorklistViewModel model = new HccWorklistViewModel()
            {
                WorklistEntries = await this._hccLogic.Value.SelectAllPeerReviews(),
                CurrentUser = this._userLogic.Value.GetUserByEmail($"{HttpContext.Session["Username"].ToString()}{"@summithealthcare.com"}")
            };
            model.WorklistEntries.ForEach(x => x.IsReadOnly = true);


            if (model.WorklistEntries.Count > 0)
            {
                int i = 0;
                while (i < model.WorklistEntries.Count)
                {
                    WorklistEntry decisionModel = new WorklistEntry();
                    decisionModel.ClaimID = model.WorklistEntries[i].ClaimID;
                    decisionModel = this._hccLogic.Value.DecisionSetUp(decisionModel);
                    if (decisionModel.agree != false)
                    {
                        model.WorklistEntries[i].decision = "Agreed";
                    }
                    else if (decisionModel.disagree != false)
                    {
                        model.WorklistEntries[i].decision = "Disagree";
                    }

                    i++;
                }

            }
            return PartialView("partials/_AllPeerReviews", model);
        }

        public async Task<ActionResult> AllCompletedPeerReviewSelected(string ChargeID)
        {
            HccWorkEntryViewModel model = await PopulateHccWorkEntryViewModel(ChargeID, false);
            string email = $"{HttpContext.Session["Username"].ToString()}{"@summithealthcare.com"}";
            model.CurrentUser = this._userLogic.Value.GetUserByEmail(email);
            HccWorklistViewModel boolModel = new HccWorklistViewModel();
            boolModel.WorklistEntries = await this._hccLogic.Value.SelectBoolsFromDatabase(model);
            if (boolModel.WorklistEntries[0].agree == true)
            {
                model.Entry.decision = "Agreed";
                model.Entry.notes = null;

            }
            else if (boolModel.WorklistEntries[0].disagree == true)
            {
                model.Entry.decision = "Disagreed";
                model.Entry.notes = boolModel.WorklistEntries[0].notes;
            }



            return View(model);
        }

    }
}