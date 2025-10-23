using RiskAdjustment.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskAdjustment.z.DbSeeder
{
    public class PvpAuditSeeder
    {
        public string sql = "CREATE TABLE Audit_Athena_PVP_Worklist (" +
            "Id INTEGER IDENTITY(1,1)," +
            "AppointmentID INTEGER," +
            "StartTime TIME(7) NULL," +
            "HCC01Code VARCHAR(7) NULL," +
            "HCC02Code VARCHAR(7) NULL," +
            "HCC03Code VARCHAR(7) NULL," +
            "HCC04Code VARCHAR(7) NULL," +
            "HCC05Code VARCHAR(7) NULL," +
            "HCC01 VARCHAR(7) NULL," +
            "HCC02 VARCHAR(7) NULL," +
            "HCC03 VARCHAR(7) NULL," +
            "HCC04 VARCHAR(7) NULL," +
            "HCC05 VARCHAR(7) NULL," +
            "HCC6plus BIT NULL," +
            "PortalReview BIT NULL," +
            "PVPNoteAdded BIT NULL," +
            "NoNoteReason VARCHAR(50) NULL," +
            "Reviewer VARCHAR(50) NULL," +
            "ReviewDate DATETIME NULL," +
            "Note VARCHAR(MAX) NULL," +
            "CPT_99499_ICD VARCHAR(7) NULL," +
            "CPT_99499_DOS DATETIME NULL," +
            "PreviousPVPDate DATETIME NULL," +
            "ArrivedApptDate DATETIME NULL," +
            "Rework_Reviewer VARCHAR(5) NULL," +
            "Rework_Date DATETIME NULL," +
            "Rework_HCCProblems BIT NULL," +
            "Rework_HCCAddressed BIT NULL," +
            "EmailNotification BIT NULL," +
            "BannerAlertAdded BIT NULL," +
            "patdecdate DATETIME NULL," +
            "AuditDate DATETIME NULL," +
            "Auditor VARCHAR(50) NULL," +
            "AuditResult VARCHAR(30) NULL," +
            "AuditNote VARCHAR(255) NULL," +
            "PatientCaseSent BIT NULL," +
            "ReworkNote VARCHAR(255) NULL," +
            "Rework99429_ICD VARCHAR(7)," +
            "Rework99429_Date DATETIME NULL," +
            "DxSubmittedOnClaim BIT NULL," +
            "ProblemListUpdate BIT NULL," +
            "PatientCaseDate DATETIME NULL," +
            "PatientCaseCode01 VARCHAR(7) NULL," +
            "PatientCaseCode02 VARCHAR(7) NULL," +
            "PatientCaseCode03 VARCHAR(7) NULL," +
            "PatientCaseCode04 VARCHAR(7) NULL," +
            "PatientCaseCode05 VARCHAR(7) NULL," +
            "PatientCaseHCC01 VARCHAR(4) NULL," +
            "PatientCaseHCC02 VARCHAR(4) NULL," +
            "PatientCaseHCC03 VARCHAR(4) NULL," +
            "PatientCaseHCC04 VARCHAR(4) NULL," +
            "PatientCaseHCC05 VARCHAR(4) NULL," +
            "PatientCaseHCCReplaced01 VARCHAR(4) NULL," +
            "PatientCaseHCCReplaced02 VARCHAR(4) NULL," +
            "PatientCaseHCCReplaced03 VARCHAR(4) NULL," +
            "PatientCaseHCCReplaced04 VARCHAR(4) NULL," +
            "PatientCaseHCCReplaced05 VARCHAR(4) NULL," +
            "AuditId INTEGER," +
            "AuditUser VARCHAR(50)," +
            "AuditAction VARCHAR(50)," +
            "AuditDateTime DATETIME)";

        public void Seed()
        {
            DbQuery query = new DbQuery(DbType.PVP);
            query.ExecuteCommand(sql);
        }
    }
}
