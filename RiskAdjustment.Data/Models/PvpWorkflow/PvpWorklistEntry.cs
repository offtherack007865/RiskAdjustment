using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskAdjustment.Data.Attributes;
using RiskAdjustment.Data.Models.RefTables;

// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

namespace RiskAdjustment.Data.Models.PvpWorkflow
{
    public class PvpWorklistEntry
    {
        private string _startTime;

        [DbColumn("")]
        public int AppointmentID { get; set; }
        [DbColumn("")]
        public string AppointmentSite { get; set; }
        [DbColumn("")]        
        public int Appt_POS_ID { get; set; }
        [DbColumn("")]
        public string AppointmentProvider { get; set; }
        [DbColumn("")]
        public int Appt_Prov_ID { get; set; }
        [DbColumn("")]
        public int PatientID { get; set; }
        [DbColumn("")]
        public string GM_Acctno { get; set; }
        [DbColumn("")]
        public string PatientName { get; set; }
        [DbColumn("")]
        public DateTime? patdob { get; set; }
        [DbColumn("")]
        public DateTime? NextApptDate { get; set; }
        [DbColumn("")]
        public string NextApptTime { get; set; }
        [DbColumn("")]
        public DateTime? PVPApptDate { get; set; }
        [DbColumn("")]
        public string PVPApptTime { get; set; }
        [DbColumn("")]
        public int AppointmentTypeID { get; set; }
        [DbColumn("")]
        public string AppointmentTypeName { get; set; }
        [DbColumn("")]
        public string AppointmentTypeShortName { get; set; }
        [DbColumn("")]
        public string AppointmentStatus { get; set; }
        [DbColumn("")]
        public string ChiefComplaint { get; set; }
        [DbColumn("")]
        public string Contract { get; set; }
        [DbColumn("")]
        public string PrimaryInsurancePackageName { get; set; }
        [DbColumn("")]
        public string PrimaryInsID { get; set; }
        [DbColumn("")]
        public int PcpCode { get; set; }
        [DbColumn("")]
        public string PcpName { get; set; }
        [DbColumn("")]
        public int PcpSiteCode { get; set; }
        [DbColumn("")]
        public string PcpSiteName { get; set; }
        [DbColumn("")]
        public DateTime? ApptsLastUpdate { get; set; }
        [DbColumn("")]
        public DateTime? FileDate { get; set; }

        // pwm - 20240525 - Add Priority.
        [DbColumn("")]
        public int Priority { get; set; }

        public TimeSpan? StartTime { get; set; }

        public string HCC01Code { get; set; }

        public string HCC02Code { get; set; }

        public string HCC03Code { get; set; }

        public string HCC04Code { get; set; }

        public string HCC05Code { get; set; }

        public string HCC01 { get; set; }

        public string HCC02 { get; set; }

		public string HCC03 { get; set; }

        public string HCC04 { get; set; }

        public string HCC05 { get; set; }

        public string HCC06Code { get; set; }
        public string HCC06 { get; set; }
        public string HCC07Code { get; set; }
        public string HCC07 { get; set; }
        public string HCC08Code { get; set; }
        public string HCC08 { get; set; }

        public string SuspectHCC01Code { get; set; }
        public string SuspectHCC01 { get; set; }
        public string SuspectHCC02Code { get; set; }
        public string SuspectHCC02 { get; set; }
        public string SuspectHCC03Code { get; set; }
        public string SuspectHCC03 { get; set; }
        public string SuspectHCC04Code { get; set; }
        public string SuspectHCC04 { get; set; }
        public string SuspectHCC05Code { get; set; }
        public string SuspectHCC05 { get; set; }
        public string SuspectHCC06Code { get; set; }
        public string SuspectHCC06 { get; set; }
        public string SuspectHCC07Code { get; set; }
        public string SuspectHCC07 { get; set; }
        public string SuspectHCC08Code { get; set; }
        public string SuspectHCC08 { get; set; }

        public bool HCC6plus { get; set; }

        public bool PortalReview { get; set; }

        public int PVPNoteAdded { get; set; }

        public string NoNoteReason { get; set; }

        public string Reviewer { get; set; }

        public DateTime? ReviewDate { get; set; }

        public string ReviewNote { get; set; }

        public string CPT_99499_ICD { get; set; }

        public DateTime? CPT_99499_DOS { get; set; }

        public DateTime? PreviousPVPDate{ get; set; }
        public DateTime? ArrivedApptDate { get; set; }

        public string Rework_Reviewer { get; set; }

        public DateTime? Rework_Date { get; set; }

        public bool Rework_HCCProblems { get; set; }

        public bool Rework_HCCAddressed { get; set; }

        public bool EmailNotification { get; set; }

        public bool BannerAlertAdded { get; set; }

        public DateTime? patdecdate { get; set; }

        public DateTime? AuditDate { get; set; }

        public string Auditor { get; set; }

        public string AuditResult { get; set; }

        public string AuditNote { get; set; }

        public bool PatientCaseSent { get; set; }

        public string ReworkNote { get; set; }

        public string Rework99429_ICD { get; set; }

        public DateTime? Rework99429_Date { get; set; }

        public bool DxSubmittedOnClaim { get; set; }

        public bool ProblemListUpdate { get; set; }

        public DateTime? PatientCaseDate { get; set; }

        public string PatientCaseCode01 { get; set; }

        public string PatientCaseCode02 { get; set; }

        public string PatientCaseCode03 { get; set; }

        public string PatientCaseCode04 { get; set; }

        public string PatientCaseCode05 { get; set; }

        public string PatientCaseHCC01 { get; set; }

        public string PatientCaseHCC02 { get; set; }

        public string PatientCaseHCC03 { get; set; }

        public string PatientCaseHCC04 { get; set; }

        public string PatientCaseHCC05 { get; set; }

        public string PatientCaseHCCReplaced01 { get; set; }

        public string PatientCaseHCCReplaced02 { get; set; }

        public string PatientCaseHCCReplaced03 { get; set; }

        public string PatientCaseHCCReplaced04 { get; set; }

        public string PatientCaseHCCReplaced05 { get; set; }

        public string HCC09Code { get; set; }
        public string HCC09 { get; set; }
        public string HCC10Code { get; set; }
        public string HCC10 { get; set; }
        public string OpenGapHCC01Code { get; set; }
        public string OpenGapHCC01 { get; set; }
        public string OpenGapHCC02Code { get; set; }
        public string OpenGapHCC02 { get; set; }
        public string OpenGapHCC03Code { get; set; }
        public string OpenGapHCC03 { get; set; }
        public string OpenGapHCC04Code { get; set; }
        public string OpenGapHCC04 { get; set; }
        public string OpenGapHCC05Code { get; set; }
        public string OpenGapHCC05 { get; set; }
        public string OpenGapHCC06Code { get; set; }
        public string OpenGapHCC06 { get; set; }
        public string OpenGapHCC07Code { get; set; }
        public string OpenGapHCC07 { get; set; }
        public string OpenGapHCC08Code { get; set; }
        public string OpenGapHCC08 { get; set; }
        public string OpenGapHCC09Code { get; set; }
        public string OpenGapHCC09 { get; set; }
        public string OpenGapHCC10Code { get; set; }
        public string OpenGapHCC10 { get; set; }
        public bool Dismissed { get; set; }
        public DateTime DismissedDate { get; set; }
        public string DismissedHCC01Code { get; set; }
        public string DismissedHCC01 { get; set; }
        public string DismissedHCC02Code { get; set; }
        public string DismissedHCC02 { get; set; }
        public string DismissedHCC03Code { get; set; }
        public string DismissedHCC03 { get; set; }
        public string DismissedHCC04Code { get; set; }
        public string DismissedHCC04 { get; set; }
        public string DismissedHCC05Code { get; set; }
        public string DismissedHCC05 { get; set; }
        public string CorrectedClaims { get; set; }
        public string CorrectedClaimsNumber { get; set; }
        public string CorrectedClaimsComments { get; set; }

        //Non-Db fields

        public string AppointmentDateTime
        {
            get
            {
                return this.PVPApptDate.Value.ToShortDateString() + " " + this.PVPApptTime;
            }
            set { }
        }

        public DateTime StartDate { get; set; }
        public string HccGapsThisYear { get; set; }
        public string HccGapsLastYear { get; set; }
        public User LockedBy { get; set; }
        public bool IsReadOnly { get; set; }
        public bool SendToReviewsPending { get; set; }
        //public bool AddToPending { get; set; }
        public int LockId { get; set; }
        public bool InPendingBucket { get; set; }
        public bool HighlightedProvider { get; set; }
	}
}
