using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskAdjustment.Data.Attributes;
using RiskAdjustment.Data.Models.RefTables;


namespace RiskAdjustment.Data.Models
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class WorklistEntry
    {
        //TODO:  Get HCC Remove fields from Carol
        private string _ptFullName;
        //Start DB Fields

        [DbColumn("patientid")]
        public int PatientID { get; set; }
        [DbColumn("claimid")]
        public int ClaimID { get; set; }
        [DbColumn("chargeid")]
        public int ChargeID { get; set; }
        [DbColumn("chrpostdate")]
        public DateTime ChargePostDate { get; set; }
        [DbColumn("proccode")]
        public string ProcCode { get; set; } = string.Empty;
        [DbColumn("clmsvcdeptid")]
        public int ClmsvcdeptID { get; set; }
        [DbColumn("svc dept bill name")]
        public string DepartmentBillingName { get; set; } = string.Empty;
        [DbColumn("rndrng prvdrid")]
        public int RndrngProviderID { get; set; }
        [DbColumn("rndrng prvdrfullnme")]
        public string ProviderFullName { get; set; } = string.Empty;
        [DbColumn("patient lastname")]
        public string PatientLastName { get; set; } = string.Empty;
        [DbColumn("patient firstname")]
        public string PatientFirstName { get; set; } = string.Empty;
        [DbColumn("clmdxcode01")]
        public string ICD10ClaimDiagCode01 { get; set; } = string.Empty;
        [DbColumn("clmdxcode02")]
        public string ICD10ClaimDiagCode02 { get; set; } = string.Empty;
        [DbColumn("clmdxcode03")]
        public string ICD10ClaimDiagCode03 { get; set; } = string.Empty;
        [DbColumn("clmdxcode04")]
        public string ICD10ClaimDiagCode04 { get; set; } = string.Empty;
        [DbColumn("clmdxcode05")]
        public string ICD10ClaimDiagCode05 { get; set; } = string.Empty;
        [DbColumn("clmdxcode06")]
        public string ICD10ClaimDiagCode06 { get; set; } = string.Empty;
        [DbColumn("clmdxcode07")]
        public string ICD10ClaimDiagCode07 { get; set; } = string.Empty;
        [DbColumn("clmdxcode08")]
        public string ICD10ClaimDiagCode08 { get; set; } = string.Empty;
        [DbColumn("clmdxcode09")]
        public string ICD10ClaimDiagCode09 { get; set; } = string.Empty;
        [DbColumn("clmdxcode10")]
        public string ICD10ClaimDiagCode10 { get; set; } = string.Empty;
        [DbColumn("clmdxcode11")]
        public string ICD10ClaimDiagCode11 { get; set; } = string.Empty;
        [DbColumn("clmdxcode12")]
        public string ICD10ClaimDiagCode12 { get; set; } = string.Empty;
        [DbColumn("cstm ins grpng")]
        public string CstmInsGrpng { get; set; } = string.Empty;
        [DbColumn("patient primary ins pkg id")]
        public int? PatientPrimaryInsPkgID { get; set; }
        [DbColumn("patient primary ins pkg name")]
        public string PatientPrimaryInsPkgName { get; set; } = string.Empty;
        [DbColumn("patient primary ins pkg type")]
        public string PatientPrimaryInsPkgType { get; set; } = string.Empty;
        [DbColumn("patient primary policyidnumber")]
        public string PatientPrimaryPolicyIDNumber { get; set; } = string.Empty;
        [DbColumn("claimstatus")]
        public string ClaimStatus { get; set; } = string.Empty;
        [DbColumn("primary Claimstatus")]
        public string PrimaryClaimstatus { get; set; } = string.Empty;
        [DbColumn("secondary Claimstatus")]
        public string SecondaryClaimstatus { get; set; } = string.Empty;
        [DbColumn("wklst assgnd usr")]
        public string WklstAssignedUser { get; set; } = string.Empty;
        [DbColumn("days in hold status")]
        public int? DaysInHoldStatus { get; set; }
        [DbColumn("wklst escalated")]
        public string WklstEscalated { get; set; } = string.Empty;
        [DbColumn("wklst name")]
        public string WklstName { get; set; } = string.Empty;
        [DbColumn("wklst note")]
        public string WklstNote { get; set; } = string.Empty;
        [DbColumn("wklst status date")]
        public DateTime? WklstStatusDate { get; set; }
        [DbColumn("wklst team name")]
        public string WklstTeamName { get; set; } = string.Empty;
        [DbColumn("curr cstm rule")]
        public string CurrCstmRule { get; set; } = string.Empty;
        [DbColumn("transactivtyid")]
        public int? TransactivityID { get; set; }
        [DbColumn("trnsctn postdate")]
        public DateTime? TransactionPostDate { get; set; }
        [DbColumn("trnsctn pstvd")]
        public string TransactionPstvd { get; set; } = string.Empty;
        [DbColumn("trnsctn srvdate")]
        public DateTime? TransactionSrvDate { get; set; }
        [DbColumn("trnsctn type")]
        public string TransactionType { get; set; } = string.Empty;
        [DbColumn("trnsctn voideddate")]
        public DateTime? TransactionVoidedDate { get; set; }
        [DbColumn("trnsctn vdd ysn")]
        public string TransactionVddYsn { get; set; } = string.Empty;
        [DbColumn("ptnt gm mrn")]
        public int? PatientGMMRN { get; set; }
        [DbColumn("ptnt grnwy mrn")]
        public int? PatientGreenwayMRN { get; set; }
        [DbColumn("ptnt op mrn")]
        public int? PatientOPMRN { get; set; }
        [DbColumn("ptnt lgcy ptnt id")]
        public int? PatientLegacyPatientID { get; set; }
        [DbColumn("FileDate")]
        public DateTime FileDate { get; set; } //= Convert.ToDateTime(DateTime.Now.ToString(@"MM-dd-yyyy"));
        [DbColumn("agree")]
        public bool agree { get; set; }
        [DbColumn("disagree")]
        public bool disagree { get; set; }
        [DbColumn("manager_completed")]
        public bool manager_completed { get; set; }
        [DbColumn("notes")]

        public string notes { get; set; }

        public string decision { get; set; }


        public string Contract { get; set; }
        public string MemberList { get; set; }
        public string ClaimPriority { get; set; }
        public string ChargePriority { get; set; }
        public string Reviewer { get; set; }
        public DateTime? ReviewDate { get; set; }
        public string ReviewNote { get; set; }
        public TimeSpan? StartTime { get; set; }
        public string HCC_ADD_DX01 { get; set; }
        public string HCC_ADD_DX01_Reason { get; set; }
        public string HCC_ADD_DX02 { get; set; }
        public string HCC_ADD_DX02_Reason { get; set; }
        public string HCC_ADD_DX03 { get; set; }
        public string HCC_ADD_DX03_Reason { get; set; }
        public string HCC_ADD_DX04 { get; set; }
        public string HCC_ADD_DX04_Reason { get; set; }
        public string HCC_ADD_DX05 { get; set; }
        public string HCC_ADD_DX05_Reason { get; set; }
        public string HCC_ADD_DX06 { get; set; }
        public string HCC_ADD_DX06_Reason { get; set; }
        public string HCC_ADD_DX07 { get; set; }
        public string HCC_ADD_DX07_Reason { get; set; }
        public string HCC_ADD_DX08 { get; set; }
        public string HCC_ADD_DX08_Reason { get; set; }
        public string HCC_ADD_DX09 { get; set; }
        public string HCC_ADD_DX09_Reason { get; set; }
        public string HCC_ADD_DX10 { get; set; }
        public string HCC_ADD_DX10_Reason { get; set; }
        public string HCC_ADD_DX11 { get; set; }
        public string HCC_ADD_DX11_Reason { get; set; }
        public string HCC_ADD_DX12 { get; set; }
        public string HCC_ADD_DX12_Reason { get; set; }
        public string NonHCC_ADD_DX01 { get; set; }
        public string NonHCC_ADD_DX02 { get; set; }
        public string NonHCC_ADD_DX03 { get; set; }
        public string NonHCC_ADD_DX04 { get; set; }
        public string HCC_REM_DX01 { get; set; }
        public string HCC_REM_DX02 { get; set; }
        public string HCC_REM_DX03 { get; set; }
        public string HCC_REM_DX04 { get; set; }
        public string NonHCC_REM_DX01 { get; set; }
        public string NonHCC_REM_DX02 { get; set; }
        public string NonHCC_REM_DX03 { get; set; }
        public string NonHCC_REM_DX04 { get; set; }
        public bool Hold { get; set; }
        public string Hold_Reason { get; set; }
        public string Hold_Note { get; set; }
        public string Provider_Response { get; set; }
        public bool PreviouslyReviewed { get; set; }
        public string AddtoIssueListBy { get; set; }
        public DateTime? AddtoIssueListDate { get; set; }
        public string IssueDXCode01 { get; set; }
        public string IssueCategory01 { get; set; }
        public string IssueDXCode02 { get; set; }
        public string IssueCategory02 { get; set; }
        public string IssueDXCode03 { get; set; }
        public string IssueCategory03 { get; set; }
        public string IssueDXCode04 { get; set; }
        public string IssueCategory04 { get; set; }
        public string IssueDXCode05 { get; set; }
        public string IssueCategory05 { get; set; }
        public string IssueDisposition { get; set; }
        public string IssueDisposition2 { get; set; }
        public string IssueDisposition3 { get; set; }
        public string IssueDisposition4 { get; set; }
        public string IssueDisposition5 { get; set; }
        public string IssueClosedBy { get; set; }
        public DateTime? IssueClosedDate { get; set; }
        public string TaskToWhom { get; set; }
        public string TaskCreation { get; set; }
        public string TaskOutcome { get; set; }
        public string Auditor { get; set; }
        public DateTime? AuditDate { get; set; }
        public string AuditResult { get; set; }
        public string AuditNote { get; set; }
        public string CPT_99499_ICD { get; set; }
        public DateTime? CPT_99499_DOS { get; set; }
        public int ClaimHCC_V2x_Count { get; set; }
        public int ClaimHCC_ESRD_Count { get; set; }
        public bool RptCat_HCC_Add { get; set; }
        public int RptCat_HCC_Add_Count { get; set; }
        public bool RptCat_HCC_Remove { get; set; }
        public int RptCat_HCC_Remove_Count { get; set; }
        public bool RptCat_HCC_NoChange { get; set; }
        public bool RptCat_NonHCC_Change { get; set; }
        public int RptCat_HCC_Add_New { get; set; }
        public int RptCat_HCC_Add_Specificity { get; set; }
        public bool RptCat_Hold { get; set; }
        public bool RptCat_ChargeErrorCorrected { get; set; }
        public bool HCCImpacted { get; set; }
        public bool Rptcat_PreviouslyReviewed { get; set; }
        public bool TaskEscalate { get; set; }
        public DateTime? TaskEscalateDate { get; set; }
        public bool DemandBilled { get; set; }

        //Non-DB fields for processing
        public int Priority { get; set; }
        public string PatientFullName
        {
            get { return $"{PatientLastName}, {PatientFirstName}"; }
        }

        public User LockedBy { get; set; }
        public bool IsReadOnly { get; set; }
        public bool SendToReviewsPending { get; set; }
        //public bool AddToPending { get; set; }
        public int LockId { get; set; }
        public bool InPendingBucket { get; set; }



    }
}
