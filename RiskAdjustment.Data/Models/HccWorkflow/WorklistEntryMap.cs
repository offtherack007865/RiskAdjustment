using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper.FluentMap.Mapping;



namespace RiskAdjustment.Data.Models.HccWorkflow
{
    public class WorklistEntryMap : EntityMap<WorklistEntry>
    {
        public WorklistEntryMap()
        {
            Map(p => p.PatientID).ToColumn("patientid");
            Map(p => p.ClaimID).ToColumn("claimid");
            Map(p => p.ChargeID).ToColumn("chargeid");
            Map(p => p.ChargePostDate).ToColumn("chrpostdate");
            Map(p => p.ProcCode).ToColumn("proccode");
            Map(p => p.ClmsvcdeptID).ToColumn("clmsvcdeptid");
            Map(p => p.DepartmentBillingName).ToColumn("svc dept bill name");
            Map(p => p.RndrngProviderID).ToColumn("rndrng prvdrid");
            Map(p => p.ProviderFullName).ToColumn("rndrng prvdrfullnme");
            Map(p => p.PatientLastName).ToColumn("patient lastname");
            Map(p => p.PatientFirstName).ToColumn("patient firstname");
            Map(p => p.ICD10ClaimDiagCode01).ToColumn("clmdxcode01");
            Map(p => p.ICD10ClaimDiagCode02).ToColumn("clmdxcode02");
            Map(p => p.ICD10ClaimDiagCode03).ToColumn("clmdxcode03");
            Map(p => p.ICD10ClaimDiagCode04).ToColumn("clmdxcode04");
            Map(p => p.ICD10ClaimDiagCode05).ToColumn("clmdxcode05");
            Map(p => p.ICD10ClaimDiagCode06).ToColumn("clmdxcode06");
            Map(p => p.ICD10ClaimDiagCode07).ToColumn("clmdxcode07");
            Map(p => p.ICD10ClaimDiagCode08).ToColumn("clmdxcode08");
            Map(p => p.ICD10ClaimDiagCode09).ToColumn("clmdxcode09");
            Map(p => p.ICD10ClaimDiagCode10).ToColumn("clmdxcode10");
            Map(p => p.ICD10ClaimDiagCode11).ToColumn("clmdxcode11");
            Map(p => p.ICD10ClaimDiagCode12).ToColumn("clmdxcode12");
            Map(p => p.CstmInsGrpng).ToColumn("cstm ins grpng");
            Map(p => p.PatientPrimaryInsPkgID).ToColumn("patient primary ins pkg id");
            Map(p => p.PatientPrimaryInsPkgName).ToColumn("patient primary ins pkg name");
            Map(p => p.PatientPrimaryInsPkgType).ToColumn("patient primary ins pkg type");
            Map(p => p.PatientPrimaryPolicyIDNumber).ToColumn("patient primary policyidnumber");
            Map(p => p.ClaimStatus).ToColumn("claimstatus");
            Map(p => p.PrimaryClaimstatus).ToColumn("primary Claimstatus");
            Map(p => p.SecondaryClaimstatus).ToColumn("secondary Claimstatus");
            Map(p => p.WklstAssignedUser).ToColumn("wklst assgnd usr");
            Map(p => p.DaysInHoldStatus).ToColumn("days in hold status");
            Map(p => p.WklstEscalated).ToColumn("wklst escalated");
            Map(p => p.WklstName).ToColumn("wklst name");
            Map(p => p.WklstNote).ToColumn("wklst note");
            Map(p => p.WklstStatusDate).ToColumn("wklst status date");
            Map(p => p.WklstTeamName).ToColumn("wklst team name");
            Map(p => p.CurrCstmRule).ToColumn("curr cstm rule");
            Map(p => p.TransactivityID).ToColumn("transactivt0yid");
            Map(p => p.TransactionPostDate).ToColumn("trnsctn postdate");
            Map(p => p.TransactionPstvd).ToColumn("trnsctn pstvd");
            Map(p => p.TransactionSrvDate).ToColumn("trnsctn srvdate");
            Map(p => p.TransactionType).ToColumn("trnsctn type");
            Map(p => p.TransactionVoidedDate).ToColumn("trnsctn voideddate");
            Map(p => p.TransactionVddYsn).ToColumn("ptnt vdd ysn");
            Map(p => p.PatientGMMRN).ToColumn("ptnt gm mrn");
            Map(p => p.PatientGreenwayMRN).ToColumn("ptnt grnwy mrn");
            Map(p => p.PatientOPMRN).ToColumn("ptnt op mrn");
            Map(p => p.FileDate).ToColumn("FileDate");
            Map(p => p.agree).ToColumn("agree");
            Map(p => p.disagree).ToColumn("disagree");
            Map(p => p.manager_completed).ToColumn("manager_completed");
        }
      
    }
}
