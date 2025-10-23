using RiskAdjustment.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskAdjustment.z.DbSeeder
{
    public class HccWorker
    {
		public string sql = "CREATE TABLE Audit_HCC_Athena_Worklist (" +
							"Id INTEGER IDENTITY(1,1)," +
							"ChargeID INTEGER," +
							"[Reviewer] [varchar] (50) NULL," +
							"[ReviewDate] [datetime] NULL," +
							"[StartTime] [time] (7) NULL," +
							"[HCC_ADD_DX01] [varchar] (10) NULL," +
							"[HCC_ADD_DX02] [varchar] (10) NULL," +
							"[HCC_ADD_DX03] [varchar] (10) NULL," +
							"[HCC_ADD_DX04] [varchar] (10) NULL," +
							"[HCC_ADD_DX05] [varchar] (10) NULL," +
							"[HCC_ADD_DX06] [varchar] (10) NULL," +
							"[HCC_ADD_DX07] [varchar] (10) NULL," +
							"[HCC_ADD_DX08] [varchar] (10) NULL," +
							"[HCC_ADD_DX09] [varchar] (10) NULL," +
							"[HCC_ADD_DX10] [varchar] (10) NULL," +
							"[HCC_ADD_DX11] [varchar] (10) NULL," +
							"[HCC_ADD_DX12] [varchar] (10) NULL," +
							"[HCC_ADD_DX01_Reason] [varchar] (25) NULL," +
							"[HCC_ADD_DX02_Reason] [varchar] (25) NULL," +
							"[HCC_ADD_DX03_Reason] [varchar] (25) NULL," +
							"[HCC_ADD_DX04_Reason] [varchar] (25) NULL," +
							"[HCC_ADD_DX05_Reason] [varchar] (25) NULL," +
							"[HCC_ADD_DX06_Reason] [varchar] (25) NULL," +
							"[HCC_ADD_DX07_Reason] [varchar] (25) NULL," +
							"[HCC_ADD_DX08_Reason] [varchar] (25) NULL," +
							"[HCC_ADD_DX09_Reason] [varchar] (25) NULL," +
							"[HCC_ADD_DX10_Reason] [varchar] (25) NULL," +
							"[HCC_ADD_DX11_Reason] [varchar] (25) NULL," +
							"[HCC_ADD_DX12_Reason] [varchar] (25) NULL," +
							"[NonHCC_ADD_DX01] [varchar] (10) NULL," +
							"[NonHCC_ADD_DX02] [varchar] (10) NULL," +
							"[NonHCC_ADD_DX03] [varchar] (10) NULL," +
							"[NonHCC_ADD_DX04] [varchar] (10) NULL," +
							"[HCC_REM_DX01] [varchar] (10) NULL," +
							"[HCC_REM_DX02] [varchar] (10) NULL," +
							"[HCC_REM_DX03] [varchar] (10) NULL," +
							"[HCC_REM_DX04] [varchar] (10) NULL," +
							"[NonHCC_REM_DX01] [varchar] (10) NULL," +
							"[NonHCC_REM_DX02] [varchar] (10) NULL," +
							"[NonHCC_REM_DX03] [varchar] (10) NULL," +
							"[NonHCC_REM_DX04] [varchar] (10) NULL," +
							"[Hold] [BIT]," +
							"[Hold_Reason] [varchar] (50) NULL," +
							"[PreviouslyReviewed] [BIT]," +
							"[AddtoIssueListBy] [varchar] (50) NULL," +
							"[AddtoIssueListDate] [datetime] NULL," +
							"[IssueDXCode01] [varchar] (10) NULL," +
							"[IssueCategory01] [varchar] (35) NULL," +
							"[IssueDXCode02] [varchar] (10) NULL," +
							"[IssueCategory02] [varchar] (35) NULL," +
							"[IssueDisposition] [varchar] (35) NULL," +
							"[IssueClosedBy] [varchar] (50) NULL," +
							"[IssueClosedDate] [datetime] NULL," +
							"[TaskToWhom] [varchar] (25) NULL," +
							"[TaskOutcome] [varchar] (25) NULL," +
							"[Auditor] [varchar] (50) NULL," +
							"[AuditDate] [datetime] NULL," +
							"[AuditResult] [varchar] (35) NULL," +
							"[AuditNote] [varchar] (255) NULL," +
							"[CPT_99499_ICD] [varchar] (10) NULL," +
							"[CPT_99499_DOS] [datetime] NULL," +
							"[ClaimHCC_V2x_Count] [int] NULL," +
							"[ClaimHCC_ESRD_Count] [int] NULL," +
							"[RptCat_HCC_Add] [bit] NULL," +
							"[RptCat_HCC_Add_New] INTEGER NULL," +
							"[RptCat_HCC_Add_Specificity] INTEGER NULL," +
							"[RptCat_HCC_Add_Count] [int] NULL," +
							"[RptCat_HCC_Remove] [bit] NULL," +
							"[RptCat_HCC_Remove_Count] [int] NULL," +
							"[RptCat_HCC_NoChange] [bit] NULL," +
							"[RptCat_NonHCC_Change] [bit] NULL," +
							"[RptCat_HCC_Specificity_Change] [bit] NULL," +
							"[RptCat_Hold] [bit] NULL," +
							"[RptCat_ChargeErrorCorrected] [bit] NULL," +
							"[RptCat_PreviouslyReviewed] [bit] NULL," +
							"[TaskEscalate] [bit] NULL," +
							"[TaskEscalateDate] [datetime] NULL," +
							"[DemandBilled] [bit] NULL, " +
							"AuditId INTEGER," +
							"AuditUser NVARCHAR(50)," +
							"AuditAction NVARCHAR(50)," +
							//name this audit date time to avoid duplicate column name with the Audit Date in the actual HCC Workflow
							"AuditDateTime DATETIME)";

		public void Seed()
        {
			DbQuery query = new DbQuery();
			query.ExecuteCommand(sql);
		}
	}
}
