using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using RiskAdjustment.Data;
using RiskAdjustment.Data.Models;
using RiskAdjustment.Data.Models.HccWorkflow;
using RiskAdjustment.Data.Models.RefTables;
using RiskAdjustment.Data.ViewModels.HCC;
using RiskAdjustment.Logic.Global;
using RiskAdjustment.Data.Attributes;
using Dapper;
using RiskAdjustment.Data.ViewModels;

namespace RiskAdjustment.Logic.Hcc
{

    public class HccLogic : IHccLogic
    {
        private readonly string tableName = "HCC_Athena_Worklist";
        public List<string> GetContractMenuItems()
        {
            string sql = "SELECT DISTINCT Contract FROM HCC_Athena_Worklist WHERE contract not in ('NextGenACO','CPC Plus','NoContract')";
            DbQuery query = new DbQuery();
            List<string> result = query.Execute<string>(sql);
            return result.OrderBy(q => q).ToList();
        }

        public List<ContractCount> GetContractCounts(string date = "")
        {

            if (string.IsNullOrEmpty(date))
                date = DateTime.Now.Date.ToShortDateString();
            List<ContractCount> returnValues = new List<ContractCount>();
            List<string> contracts = GetContractMenuItems();
            foreach (string s in contracts)
            {
                string sql = $"SELECT COUNT(Contract) FROM {tableName} WHERE Contract = '{s}' AND FileDate = '{DataUtilities.ConvertToSqlFormattedDateTime(date)}'";
                DbQuery query = new DbQuery();
                returnValues.Add(new ContractCount()
                {
                    ContractName = s,
                    Count = query.ExecuteSingle<int>(sql)

                });
            }
            return returnValues;
        }

        public async Task<List<WorklistEntry>> GetUnlockedWorklistEntriesForDateAsync(string date, string contract)
        {
            string convertedDate = DataUtilities.ConvertToSqlFormattedDateTime(date);
            string sql =
                $@"SELECT t1.chargeid, t1.patientid, t1.claimid, t1.ReviewDate, t1.[patient lastname] AS PatientLastName, t1.[patient firstname] AS PatientFirstName, t1.[trnsctn srvdate] AS TransactionSrvDate, t1.proccode, t1.FileDate, t1.Contract, t1.MemberList, t1.ClaimPriority, t1.ChargePriority, t1.FileDate, t1.[svc dept bill name], t1.[rndrng prvdrfullnme] FROM HCC_Athena_Worklist AS t1 " +
                $"WHERE t1.Contract = '{contract}' AND t1.FileDate = '{convertedDate}' ORDER BY ClaimPriority, claimid";
            string locksSql = @"SELECT * FROM HccRecordLocks WHERE DATEDIFF(dd, LockDateTime, CURRENT_TIMESTAMP) < 30";
            DbQuery query = new DbQuery();
            List<HccRecordLock> locks = query.Execute<HccRecordLock>(locksSql);
            Task<List<WorklistEntry>> entries = RemoveLockedEntries(await query.ExecuteAsync<WorklistEntry>(sql), locks);

            return await FilterByClaimPriority(entries);
        }

        private async Task<List<WorklistEntry>> FilterByClaimPriority(Task<List<WorklistEntry>> entries)
        {
            Task<List<WorklistEntry>> taskFilteredEntries = Task.Run(() =>
            {
                List<WorklistEntry> filteredEntries = new List<WorklistEntry>();
                var charges = entries.Result.ToList().GroupBy(x => x.ClaimID).Select(y => new { Entry = y.OrderBy(p => p.ChargePriority) });
                foreach (var i in charges)
                {
                    if (i.Entry.First().ReviewDate != null)
                        continue;
                    else
                        filteredEntries.Add(i.Entry.First());
                }
                return filteredEntries;
            });

            await Task.WhenAll(taskFilteredEntries);
            return taskFilteredEntries.Result;
        }

        private async Task<List<WorklistEntry>> FilterCompletedClaimsByPriority(Task<List<WorklistEntry>> entries)
        {
            Task<List<WorklistEntry>> taskFilteredEntries = Task.Run(() =>
            {
                List<WorklistEntry> resultEntries = new List<WorklistEntry>();
                var charges = entries.Result.ToList().GroupBy(x => x.ClaimID)
                    .Select(y => new { Entry = y.OrderBy(p => p.ChargePriority) });
                foreach (var i in charges)
                {
                    resultEntries.Add(i.Entry.First());
                }

                return resultEntries;
            });
            await Task.WhenAll(taskFilteredEntries);
            return taskFilteredEntries.Result;
        }

        //Unused.  This was replaced by the priority being set in the stored procedure.
        public async Task<List<WorklistEntry>> SetWorklistPriorityAsync(Task<List<WorklistEntry>> entries)
        {
            Task<List<WorklistEntry>> values = Task.Run(() =>
            {
                List<WorklistEntry> newEntries = new List<WorklistEntry>();
                foreach (WorklistEntry w in entries.Result)
                {
                    SetPriority(w);
                    newEntries.Add(w);
                }
                return newEntries.OrderBy(x => x.Priority).ToList();
            });
            await Task.WhenAll(values);
            return values.Result;
        }

        public async Task<WorklistEntry> GetWorklistEntryAsync(string chargeid)
        {
            //Note:  chargeid is INT in the database, however, because this field value is basically coming from the user form, it is returned as string 
            //from the view.  SQL server implicitly converts it during query execution.
            string sql =
            $"SELECT *, [patient lastname] AS PatientLastName, [patient firstname] AS PatientFirstName FROM HCC_Athena_Worklist WHERE chargeid = '{chargeid}'";
            DbQuery query = new DbQuery();
            return await query.ExecuteSingleAsync<WorklistEntry>(sql);
        }

        public async Task<WorklistEntry> GetWorklistEntryandLockAsync(string chargeid, string username)
        {

            
            GlobalUserLogic userLogic = new GlobalUserLogic();
            
            WorklistEntry entry = await GetWorklistEntryAsync(chargeid);
            if (IsClaimLocked(entry, username))
            {
                HccRecordLock lockRecord = GetActiveLockRecord(entry.ClaimID);
                User u = userLogic.GetUserByUsername(lockRecord.LockedBy);
                entry.LockedBy = u;
                //If the requesting user is the one who locked it, it should not be read only
                entry.IsReadOnly = lockRecord.LockedBy != username;
                entry.LockId = lockRecord.Id;
                if (entry.Reviewer == null)
                    entry.Reviewer = $"{u.LastName}, {u.FirstName}";
                return entry;
            }
            else
            {
                string sql =
                    $"INSERT INTO HccRecordLocks (ClaimId, LockDateTime, LockedBy) VALUES ('{entry.ClaimID}', '{DateTime.Now}', '{username}') SELECT @@IDENTITY";
                DbQuery query = new DbQuery();

                entry.LockId = await query.ExecuteScalarAsync<HccRecordLock>(sql);
                entry.LockedBy = userLogic.GetUserByUsername(username);
                entry.IsReadOnly = false;
                return entry;
            }
        }

        public async Task<WorklistEntry> GetHighestPriorityItemForClaim(int claimId)
        {
            Task<WorklistEntry> resultEntry = Task.Run(async () =>
            {
                WorklistEntry entries = new WorklistEntry();
                string sql = $"SELECT TOP(1) * FROM HCC_Athena_Worklist WHERE claimid = {claimId} ORDER BY ChargePriority";
                DbQuery query = new DbQuery();
                entries = await query.ExecuteSingleAsync<WorklistEntry>(sql);
                return entries;
            });
            await Task.FromResult(resultEntry);
            return resultEntry.Result;
            //I probably should create a 'GetClaim' method, but i just don't want to right now

        }

        public async Task<List<WorklistEntry>> GetRelatedCptsAsync(int claimid, int currentChargeId, int patientId, DateTime? dateOfService, string placeOfService)
        {
            Task<List<WorklistEntry>> entriesResult = Task.Run(async () =>
            {
                string sql = $"SELECT *, [patient lastname] AS PatientLastName, [patient firstname] AS PatientFirstName FROM HCC_Athena_Worklist WHERE claimid = {claimid} AND chargeid <> {currentChargeId}";
                //VBC wants to also look for CPTs that the same patient ID, DOS, and POS.
                string additionalSql = $"SELECT *, [patient lastname] AS PatientLastName, [patient firstname] AS PatientFirstName FROM HCC_Athena_Worklist WHERE patientid = {patientId} AND [trnsctn srvdate] = '{dateOfService}' AND [svc dept bill name] = '{placeOfService}' AND claimid <> {claimid}";
                DbQuery query = new DbQuery();
                List<WorklistEntry> entries = await query.ExecuteAsync<WorklistEntry>(sql);
                entries.AddRange(await query.ExecuteAsync<WorklistEntry>(additionalSql));
                return entries;
            });
            await Task.FromResult(entriesResult);
            return entriesResult.Result;
        }

        public async void ClearUserLockForCharge(string username, string chargeid)
        {
            await Task.Run(() =>
            {
                string sql = $"UPDATE HccRecordLocks SET LockReleaseDateTime = '{DateTime.Now}', LockReleasedBy = '{username} (auto)' WHERE LockedBy = '{username}' AND chargeid = {Convert.ToInt32(chargeid) }";
                DbQuery query = new DbQuery();
                query.ExecuteCommandAsync(sql);
            });
            await Task.CompletedTask;
        }

        public async void ClearUserLockForCharge(int lockId, string username)
        {
            _ = Task.Run(() =>
              {
                  string sql = $"UPDATE HccRecordLocks SET LockReleaseDateTime = '{DateTime.Now}', LockReleasedBy = '{username} (auto)', Pending = 0 WHERE Id = {lockId}";
                  DbQuery query = new DbQuery();
                  query.ExecuteCommandAsync(sql);

              });
            await Task.CompletedTask;
        }

        public async void ClearAllRecordLocksForUserAsync(string userName)
        {
            await Task.Run(() =>
            {
                string sql =
                   $"UPDATE HccRecordLocks SET LockReleaseDateTime = '{DateTime.Now}', LockReleasedBy = '{userName} (auto)' WHERE LockedBy = '{userName}' AND Pending <> 1";
                DbQuery query = new DbQuery();
                query.ExecuteCommandAsync(sql);
                return Task.CompletedTask;
            });
        }

        private async Task<List<WorklistEntry>> RemoveLockedEntries(List<WorklistEntry> entries, List<HccRecordLock> locks)
        {
            Task<List<WorklistEntry>> resultsTask = Task.Run(() =>
            {
                List<WorklistEntry> results = new List<WorklistEntry>();
                foreach (WorklistEntry entry in entries)
                {
                    if (!IsClaimLocked(entry, locks))
                        results.Add(entry);
                }

                return results;
            });

            await Task.WhenAll(resultsTask);
            return resultsTask.Result;
        }

        public bool IsClaimLocked(WorklistEntry entry, string username)
        {
            //string locksSql = @"SELECT * FROM HccRecordLocks WHERE DATEDIFF(dd, LockDateTime, CURRENT_TIMESTAMP) < 30";
            string locksSql = $"SELECT * FROM HccRecordLocks WHERE ClaimId = {entry.ClaimID} AND LockReleaseDateTime IS NULL";
            DbQuery query = new DbQuery();
            List<HccRecordLock> locks = query.Execute<HccRecordLock>(locksSql);
            if (locks.Count > 0)
                return true;
            else
                return false;

            //return IsClaimLocked(entry, locks);
        }

        public bool IsClaimLocked(WorklistEntry entry, List<HccRecordLock> locks)
        {
            //
            HccRecordLock checkLock = locks.FirstOrDefault(x => Convert.ToInt32((string)x.ClaimID) == entry.ClaimID && x.LockReleaseDateTime == null);

            if (checkLock != null)
                return true;
            return false;
        }

        //Unused - this method was replaced by the priority being set in the stored procedure.
        public void SetPriority(WorklistEntry w)
        {
            if (w.ProcCode.StartsWith("993") && w.Contract == "BlueCrossMA")
                w.Priority = 2;
            else if (w.ProcCode == "99205" || w.ProcCode == "99215")
                w.Priority = 4;
            else if (w.ProcCode == "99204" || w.ProcCode == "99214")
                w.Priority = 5;
            else if (w.ProcCode == "99213" || w.ProcCode == "99200")
                w.Priority = 6;
            else if (w.ProcCode == "99202" || w.ProcCode == "99212")
                w.Priority = 7;
            else if (w.ProcCode == "99201" || w.ProcCode == "99211")
                w.Priority = 8;
            else
            {
                PropertyInfo[] properties = w.GetType().GetProperties();
                foreach (PropertyInfo prop in properties)
                {
                    if (prop.Name.StartsWith("ICD10Claim"))
                    {
                        string propValue = prop.GetValue(w, null).ToString();
                        if (string.IsNullOrEmpty(propValue))
                            continue;
                        else if (propValue.StartsWith("I63"))
                        {
                            w.Priority = 1;
                            continue;
                        }
                        else if (propValue.StartsWith("Z01") || propValue.StartsWith("Z00"))
                        {
                            w.Priority = 3;
                            continue;
                        }
                        else
                            w.Priority = 9;
                    }
                }
            }

            if (w.Priority == 0)
            {
                w.Priority = 9;
            }
        }

        public HccRecordLock GetActiveLockRecord(int claimid)
        {
            string sql = $"SELECT TOP(1) * FROM HccRecordLocks WHERE ClaimId = { claimid } AND LockReleaseDateTime IS NULL ORDER BY LockDateTime DESC";
            //string sql = $"SELECT * FROM HccRecordLocks WHERE ClaimId = '{claimid}' AND LockReleaseDateTime IS NULL";
            DbQuery query = new DbQuery();
            return query.ExecuteSingle<HccRecordLock>(sql);
        }

        public void LockRecord(int lockid)
        {
            string sql = $"UPDATE HccRecordLocks SET LockReleaseDateTime = NULL, LockReleasedBy = NULL WHERE Id = {lockid}";
            DbQuery query = new DbQuery();
            query.ExecuteCommand(sql);
        }

        public async Task<List<PreviousWorkNote>> GetPtPreviousWorkNotesAsync(int patientId)
        {
            Task<List<PreviousWorkNote>> resultNotes = Task.Run(async () =>
            {
                List<PreviousWorkNote> notes = new List<PreviousWorkNote>();
                string sql = $"SELECT FileDate, [claimid] AS ClaimId, [wklst note] AS WklstNote FROM  HCC_Athena_Worklist WHERE patientid = {patientId} AND ReviewDate IS NOT NULL AND [wklst note] IS NOT NULL AND (Year([FileDate]) = '{DateTime.Now.Year}')";
                DbQuery query = new DbQuery();
                notes = await query.ExecuteAsync<PreviousWorkNote>(sql);
                return notes;
            });
            await Task.FromResult(resultNotes);
            return resultNotes.Result;
        }

        public async Task<int> UpdateAsync(WorklistEntry entry, string auditUserName)
        {
            //Set a few columns for reporting
            entry.RptCat_HCC_Add = entry.RptCat_HCC_Add_Count > 0;
            entry.RptCat_HCC_Remove = entry.RptCat_HCC_Remove_Count > 0;
            if (entry.RptCat_HCC_Add_Count == 0 && entry.RptCat_HCC_Remove_Count == 0)
                entry.RptCat_HCC_NoChange = true;


            string sql = $@"UPDATE HCC_Athena_Worklist
            SET Reviewer = @Reviewer,
            ReviewDate = @ReviewDate,
            ReviewNote = @ReviewNote,
            StartTime = @StartTime,
            HCC_ADD_DX01 = @HCC_ADD_DX01,
            HCC_ADD_DX01_Reason = @HCC_ADD_DX01_Reason,
            HCC_ADD_DX02 = @HCC_ADD_DX02,
            HCC_ADD_DX02_Reason = @HCC_ADD_DX02_Reason,
            HCC_ADD_DX03 = @HCC_ADD_DX03,
            HCC_ADD_DX03_Reason = @HCC_ADD_DX03_Reason,
            HCC_ADD_DX04 = @HCC_ADD_DX04,
            HCC_ADD_DX04_Reason = @HCC_ADD_DX04_Reason,
            HCC_ADD_DX05 = @HCC_ADD_DX05,
            HCC_ADD_DX05_Reason = @HCC_ADD_DX05_Reason,
            HCC_ADD_DX06 = @HCC_ADD_DX06,
            HCC_ADD_DX06_Reason = @HCC_ADD_DX06_Reason,
            HCC_ADD_DX07 = @HCC_ADD_DX07,
            HCC_ADD_DX07_Reason = @HCC_ADD_DX07_Reason,
            HCC_ADD_DX08 = @HCC_ADD_DX08,
            HCC_ADD_DX08_Reason = @HCC_ADD_DX08_Reason,
            HCC_ADD_DX09 = @HCC_ADD_DX09,
            HCC_ADD_DX09_Reason = @HCC_ADD_DX09_Reason,
            HCC_ADD_DX10 = @HCC_ADD_DX10,
            HCC_ADD_DX10_Reason = @HCC_ADD_DX10_Reason,
            HCC_ADD_DX11 = @HCC_ADD_DX11,
            HCC_ADD_DX11_Reason = @HCC_ADD_DX11_Reason,
            HCC_ADD_DX12 = @HCC_ADD_DX12,
            HCC_ADD_DX12_Reason = @HCC_ADD_DX12_Reason,
            NonHCC_ADD_DX01 = @NonHCC_ADD_DX01,
            NonHCC_ADD_DX02 = @NonHCC_ADD_DX02,
            NonHCC_ADD_DX03 = @NonHCC_ADD_DX03,
            NonHCC_ADD_DX04 = @NonHCC_ADD_DX04,
            HCC_REM_DX01 = @HCC_REM_DX01,
            HCC_REM_DX02 = @HCC_REM_DX02,
            HCC_REM_DX03 = @HCC_REM_DX03,
            HCC_REM_DX04 = @HCC_REM_DX04,
            NonHCC_REM_DX01 = @NonHCC_REM_DX01,
            NonHCC_REM_DX02 = @NonHCC_REM_DX02,
            NonHCC_REM_DX03 = @NonHCC_REM_DX03,
            NonHCC_REM_DX04 = @NonHCC_REM_DX04,
            Hold = @Hold,
            Hold_Reason = @Hold_Reason,
            Hold_Note = @Hold_Note,
            Provider_Response = @Provider_Response,
            PreviouslyReviewed = @PreviouslyReviewed,
            AddtoIssueListBy = @AddtoIssueListBy,
            AddtoIssueListDate = @AddtoIssueListDate,
            IssueDXCode01 = @IssueDXCode01,
            IssueCategory01 = @IssueCategory01,
            IssueDXCode02 = @IssueDXCode02,
            IssueCategory02 = @IssueCategory02,
            IssueDXCode03 = @IssueDXCode03,
            IssueCategory03 = @IssueCategory03,
            IssueDXCode04 = @IssueDXCode04,
            IssueCategory04 = @IssueCategory04,
            IssueDXCode05 = @IssueDXCode05,
            IssueCategory05 = @IssueCategory05,
            IssueDisposition = @IssueDisposition,
            IssueDisposition2 = @IssueDisposition2,
            IssueDisposition3 = @IssueDisposition3,
            IssueDisposition4 = @IssueDisposition4,
            IssueDisposition5 = @IssueDisposition5,
            IssueClosedBy = @IssueClosedBy,
            IssueClosedDate = @IssueClosedDate,
            TaskToWhom = @TaskToWhom,
            TaskCreation = @TaskCreation,
            TaskOutcome = @TaskOutcome,
            Auditor = @Auditor,
            AuditDate = @AuditDate,
            AuditResult = @AuditResult,
            AuditNote = @AuditNote,
            CPT_99499_ICD = @CPT_99499_ICD,
            CPT_99499_DOS = @CPT_99499_DOS,
            ClaimHCC_V2x_Count = @ClaimHCC_V2x_Count,
            ClaimHCC_ESRD_Count = @ClaimHCC_ESRD_Count,
            RptCat_HCC_Add = @RptCat_HCC_Add,
            RptCat_HCC_Add_Count = @RptCat_HCC_Add_Count,
            RptCat_HCC_Remove = @RptCat_HCC_Remove,
            RptCat_HCC_Remove_Count = @RptCat_HCC_Remove_Count,
            RptCat_HCC_NoChange = @RptCat_HCC_NoChange,
            RptCat_NonHCC_Change = @RptCat_NonHCC_Change,
            RptCat_HCC_Add_New = @RptCat_HCC_Add_New,
            RptCat_HCC_Add_Specificity = @RptCat_HCC_Add_Specificity,
            RptCat_Hold = @RptCat_Hold,
            RptCat_ChargeErrorCorrected = @RptCat_ChargeErrorCorrected,
            Rptcat_PreviouslyReviewed = @Rptcat_PreviouslyReviewed,
            TaskEscalate = @TaskEscalate,
            TaskEscalateDate = @TaskEscalateDate,
            DemandBilled = @DemandBilled,
            HCCImpacted = @HCCImpacted,
            IsNavinaReview = @IsNavinaReview,
            NavinaICD10Code01 = @NavinaICD10Code01,
            NavinaICD10Code01ConditionsValidated = @NavinaICD10Code01ConditionsValidated,
            NavinaICD10Code02 = @NavinaICD10Code02,
            NavinaICD10Code02ConditionsValidated = @NavinaICD10Code02ConditionsValidated,
            NavinaICD10Code03 = @NavinaICD10Code03,
            NavinaICD10Code03ConditionsValidated = @NavinaICD10Code03ConditionsValidated,
            NavinaICD10Code04 = @NavinaICD10Code04,
            NavinaICD10Code04ConditionsValidated = @NavinaICD10Code04ConditionsValidated,
            NavinaICD10Code05 = @NavinaICD10Code05,
            NavinaICD10Code05ConditionsValidated = @NavinaICD10Code05ConditionsValidated,
            NavinaICD10Code06 = @NavinaICD10Code06,
            NavinaICD10Code06ConditionsValidated = @NavinaICD10Code06ConditionsValidated,
            NavinaICD10Code07 = @NavinaICD10Code07,
            NavinaICD10Code07ConditionsValidated = @NavinaICD10Code07ConditionsValidated,
            NavinaICD10Code08 = @NavinaICD10Code08,
            NavinaICD10Code08ConditionsValidated = @NavinaICD10Code08ConditionsValidated,
            NavinaICD10Code09 = @NavinaICD10Code09,
            NavinaICD10Code09ConditionsValidated = @NavinaICD10Code09ConditionsValidated,
            NavinaICD10Code10 = @NavinaICD10Code10,
            NavinaICD10Code10ConditionsValidated = @NavinaICD10Code10ConditionsValidated
            WHERE chargeid = @ChargeID";

            DbQuery query = new DbQuery();
            query.ActionExecuted += new EventHandler<DbQueryEventArgs>((sender, e) => AuditHccAction(sender, e, entry, DbAction.Update, auditUserName));
            return await query.ExecuteScalarAsync(sql, entry);
        }

        private async void AuditHccAction(object sender, DbQueryEventArgs e, WorklistEntry entry, DbAction action, string auditUsername)
        {
            string sql = "INSERT INTO Audit_HCC_Athena_Worklist (ChargeID, Reviewer, ReviewDate, ReviewNote, StartTime, HCC_ADD_DX01, HCC_ADD_DX01_reason, HCC_ADD_DX02, HCC_ADD_DX02_Reason, HCC_ADD_DX03, HCC_ADD_DX03_Reason, HCC_ADD_DX04, HCC_ADD_DX04_Reason, HCC_ADD_DX05, HCC_ADD_DX05_Reason, " +
                "HCC_ADD_DX06, HCC_ADD_DX06_Reason, HCC_ADD_DX07, HCC_ADD_DX07_Reason, HCC_ADD_DX08, HCC_ADD_DX08_Reason, HCC_ADD_DX09, HCC_ADD_DX09_Reason, HCC_ADD_DX10, HCC_ADD_DX10_Reason, HCC_ADD_DX11, HCC_ADD_DX11_Reason, HCC_ADD_DX12, HCC_ADD_DX12_Reason, NonHCC_ADD_DX01, NonHCC_ADD_DX02, NonHCC_ADD_DX03, " +
                "NonHCC_ADD_DX04, HCC_REM_DX01, HCC_REM_DX02, HCC_REM_DX03, HCC_REM_DX04, NonHCC_REM_DX01, NonHCC_REM_DX02, NonHCC_REM_DX03, NonHCC_REM_DX04, Hold, Hold_Reason, " +
                "PreviouslyReviewed, AddtoIssueListBy, AddtoIssueListDate, IssueDXCode01, IssueCategory01, IssueDXCode02, IssueDisposition, IssueClosedBy, " +
                "IssueClosedDate, TaskToWhom, TaskOutcome, Auditor, AuditDate, AuditResult, AuditNote, CPT_99499_ICD, CPT_99499_DOS, ClaimHCC_V2x_Count, ClaimHCC_ESRD_Count, " +
                "RptCat_HCC_Add, RptCat_HCC_Add_Count, RptCat_HCC_Remove, RptCat_HCC_Remove_Count, RptCat_HCC_NoChange, RptCat_NonHCC_Change, RptCat_HCC_Add_New, RptCat_HCC_Add_Specificity,  " +
                "RptCat_Hold, RptCat_ChargeErrorCorrected, Rptcat_PreviouslyReviewed, TaskEscalate, TaskEscalateDate, DemandBilled, AuditId, AuditUser, AuditAction, AuditDateTime, TaskCreation)" +
                "VALUES (@ChargeID, @Reviewer, @ReviewDate, @ReviewNote, @StartTime, @HCC_ADD_DX01, @HCC_ADD_DX01_Reason, @HCC_ADD_DX02, @HCC_ADD_DX02_Reason, @HCC_ADD_DX03, @HCC_ADD_DX03_Reason, @HCC_ADD_DX04, @HCC_ADD_DX04_Reason, @HCC_ADD_DX05, @HCC_ADD_DX05_Reason, @HCC_ADD_DX06, @HCC_ADD_DX06_Reason, @HCC_ADD_DX07, @HCC_ADD_DX07_Reason, " +
                "@HCC_ADD_DX08, @HCC_ADD_DX08_Reason, @HCC_ADD_DX09, @HCC_ADD_DX09_Reason, @HCC_ADD_DX10, @HCC_ADD_DX10_Reason, @HCC_ADD_DX11, @HCC_ADD_DX11_Reason, @HCC_ADD_DX12, @HCC_ADD_DX12_Reason, @NonHCC_ADD_DX01, @NonHCC_ADD_DX02, @NonHCC_ADD_DX03, @NonHCC_ADD_DX04, " +
                "@HCC_REM_DX01, @HCC_REM_DX02, @HCC_REM_DX03, @HCC_REM_DX04, @NonHCC_REM_DX01, @NonHCC_REM_DX02, @NonHCC_REM_DX03, @NonHCC_REM_DX04, @Hold, @Hold_Reason, " +
                "@PreviouslyReviewed, @AddtoIssueListBy, @AddtoIssueListDate, @IssueDXCode01, @IssueCategory01, @IssueDXCode02, @IssueDisposition, @IssueClosedBy, " +
                "@IssueClosedDate, @TaskToWhom, @TaskOutcome, @Auditor, @AuditDate, @AuditResult, @AuditNote, @CPT_99499_ICD, @CPT_99499_DOS, @ClaimHCC_V2x_Count, @ClaimHCC_ESRD_Count, " +
                "@RptCat_HCC_Add, @RptCat_HCC_Add_Count, @RptCat_HCC_Remove, @RptCat_HCC_Remove_Count, @RptCat_HCC_NoChange, @RptCat_NonHCC_Change, @RptCat_HCC_Add_New, @RptCat_HCC_Add_Specificity, " +
                $"@RptCat_Hold, @RptCat_ChargeErrorCorrected, @Rptcat_PreviouslyReviewed, @TaskEscalate, @TaskEscalateDate, @DemandBilled, @ChargeID, '{auditUsername}', " +
                $"'{DbAction.Update}', '{DateTime.Now}', @TaskCreation)";

            _ = Task.Run(async () =>
            {
                DbQuery query = new DbQuery();
                return await query.ExecuteScalarAsync(sql, entry);
            });
            await Task.CompletedTask;
        }

        public void AddToPending(int lockId)
        {
            string sql = $"UPDATE HCCRecordLocks SET LockReleaseDateTime = NULL, LockReleasedBy = NULL, Pending = 1 WHERE Id = {lockId}";
            DbQuery query = new DbQuery();
            List<HccRecordLock> recordLocks = query.Execute<HccRecordLock>(sql);
        }

        public async Task<List<WorklistEntry>> GetReviewsInProgressAsync()
        {
            Task<List<WorklistEntry>> values = Task.Run(async () =>
            {
                DbQuery query = new DbQuery();
                GlobalUserLogic userLogic = new GlobalUserLogic();
                string lockSql = "SELECT * FROM HCCRecordLocks WHERE LockReleasedBy IS NULL AND LockReleaseDateTime IS NULL";
                List<HccRecordLock> locks = await query.ExecuteAsync<HccRecordLock>(lockSql);
                List<WorklistEntry> entries = new List<WorklistEntry>();
                foreach (HccRecordLock l in locks)
                {
                    User u = userLogic.GetUserByUsername(l.LockedBy);
                    string sql = $@"SELECT t1.chargeid, t1.patientid, t1.claimid, t1.[patient lastname] AS PatientLastName, t1.[patient firstname] AS PatientFirstName, " +
                    "t1.[trnsctn srvdate] AS TransactionSrvDate, t1.proccode, t1.FileDate, t1.Contract, t1.MemberList, t1.ClaimPriority, t1.Reviewer, t1.Hold_Reason, t1.RptCat_Hold, " +
                    $"t1.ChargePriority FROM HCC_Athena_Worklist AS t1 WHERE claimid = {l.ClaimID}";
                    List<WorklistEntry> tmpEntries = await query.ExecuteAsync<WorklistEntry>(sql);
                    foreach (WorklistEntry e in tmpEntries)
                    {
                        if (e.Reviewer == null)
                            e.Reviewer = $"{u.LastName}, {u.FirstName}";
                        //e.ReviewDate = l.LockDateTime;
                    }
                    entries.AddRange(tmpEntries);
                }
                return entries;
            });

            await Task.WhenAll(values);
            List<WorklistEntry> result = await FilterByClaimPriority(values);
            return result;
        }

        public async Task<List<WorklistEntry>> GetCompleted90DaysAsync()
        {
            Task<List<WorklistEntry>> result = Task.Run(async () =>
            {
                DbQuery query = new DbQuery();
                DateTime beginningDate = DateTime.Now.AddDays(-90);
                string sql =
                    $@"SELECT t1.chargeid, t1.patientid, t1.claimid, t1.[patient lastname] AS PatientLastName, t1.[patient firstname] AS PatientFirstName, " +
                    "t1.[trnsctn srvdate] AS TransactionSrvDate, t1.proccode, t1.FileDate, t1.Contract, t1.MemberList, t1.ClaimPriority, t1.Reviewer, t1.ReviewDate, t1.Contract, t1.StartTime, t1.Hold_Reason, " +
                    $"t1.ChargePriority FROM HCC_Athena_Worklist AS t1 WHERE Reviewer IS NOT NULL AND ReviewDate IS NOT NULL AND ReviewDate > CAST('{beginningDate}' AS DATE)";
                List<WorklistEntry> taskResult = await query.ExecuteAsync<WorklistEntry>(sql);
                return taskResult;
            });
            List<WorklistEntry> filteredResults = await FilterCompletedClaimsByPriority(result);
            return filteredResults;
        }

        public async void ClearAllFieldsAsync(WorklistEntry e, string username)
        {
            List<string> excludedProperties = new List<string>()
            {
                "Contract", "MemberList", "ChargePriority", "ClaimPriority", "FileDate", "PatientFullName"
            };
            _ = Task.Run(async () =>
            {
                foreach (PropertyInfo property in e.GetType().GetProperties())
                {
                    var t = typeof(WorklistEntry);
                    var attr = (DbColumnAttribute[])property.GetCustomAttributes(typeof(DbColumnAttribute), false);
                    if (attr.Length <= 0)
                    {
                        if (excludedProperties.Contains(property.Name))
                            continue;
                        else
                            property.SetValue(e, null);
                    }
                }
                return await UpdateAsync(e, username);
            });
            await Task.CompletedTask;
        }

        public async Task<List<WorklistEntry>> PeerReviewCompleted(string filedate, string contract)
        {


            DateTime SelectedDate = new DateTime(1900, 1, 1);
            DateTime.TryParse(filedate, out SelectedDate);

            // Should come out in the format yyyyMMdd.
            string dateStringForSqlConvert =
                $"{SelectedDate.Year.ToString()}{SelectedDate.Month.ToString().PadLeft(2, '0')}{SelectedDate.Day.ToString().PadLeft(2, '0')}";

            Task<List<WorklistEntry>> result = Task.Run(async () =>
            {
                DbQuery query = new DbQuery();
                DateTime beginningDate = DateTime.Now.AddDays(-7);
                string sql =
                    $"SELECT t1.chargeid, t1.patientid, t1.claimid, t1.[patient lastname] AS PatientLastName, t1.[patient firstname] AS PatientFirstName, " +
                    $"t1.[trnsctn srvdate] AS TransactionSrvDate, t1.proccode, t1.FileDate, t1.Contract, t1.MemberList, t1.ClaimPriority, t1.Reviewer, t1.ReviewDate, t1.Contract, t1.StartTime, t1.Hold_Reason,  +" +
                    $" t1.ChargePriority FROM HCC_Athena_Worklist AS t1 WHERE Reviewer IS NOT NULL AND ReviewDate IS NOT NULL AND Contract = '{contract}' AND FileDate > dateadd(dd, -7, Convert(DateTime, '{dateStringForSqlConvert}', 112)) and FileDate <= Convert(DateTime, '{dateStringForSqlConvert}', 112) ORDER BY ChargePriority Asc";
                List<WorklistEntry> taskResult = await query.ExecuteAsync<WorklistEntry>(sql);
                return taskResult;
            });
            List<WorklistEntry> filteredResults = await FilterCompletedClaimsByPriority(result);
            return filteredResults;



        }

        public async Task<List<WorklistEntry>> selectFromDatabase(WorklistEntry model)
        {

            Task<List<WorklistEntry>> result = Task.Run(async () =>
            {
                DbQuery query = new DbQuery();
                DateTime beginningDate = DateTime.Now.AddDays(-7);
                string sql = $"SELECT * FROM  HCC_Athena_Worklist WHERE claimid = '{model.ClaimID}'";
                List<WorklistEntry> taskResult = await query.ExecuteAsync<WorklistEntry>(sql);
                return taskResult;
            });
            List<WorklistEntry> filteredResults = await FilterCompletedClaimsByPriority(result);
            return filteredResults;


        }
        public async void InsertToCompletedDatabase(WorklistEntry entry, bool decision, string sql, string note)
        {

            //if decision is agreed 
            if (decision == true)
            {
                sql = $"INSERT INTO HCC_Completed_Athena_Worklist (chargeid,chrpostdate,ChargePriority,ClaimHCC_ESRD_Count,ClaimHCC_V2x_Count,claimid,ClaimPriority,claimstatus,clmsvcdeptid,[contract],[cstm ins grpng]" +
                    $",[curr cstm rule],[days in hold status],DemandBilled,[svc dept bill name],FileDate,HCC_ADD_DX01,HCC_ADD_DX01_Reason,HCC_ADD_DX02,HCC_ADD_DX02_Reason,HCC_ADD_DX03,HCC_ADD_DX03_Reason," +
                    $"HCC_ADD_DX04,HCC_ADD_DX04_Reason,HCC_ADD_DX05,HCC_ADD_DX05_Reason,HCC_ADD_DX06,HCC_ADD_DX06_Reason,HCC_ADD_DX07,HCC_ADD_DX07_Reason,HCC_ADD_DX08,HCC_ADD_DX08_Reason,HCC_ADD_DX09,HCC_ADD_DX09_Reason," +
                    $"HCC_ADD_DX10,HCC_ADD_DX10_Reason,HCC_ADD_DX11,HCC_ADD_DX11_Reason,HCC_ADD_DX12,HCC_ADD_DX12_Reason,HCC_REM_DX01,HCC_REM_DX02,HCC_REM_DX03,HCC_REM_DX04,hold,hold_reason,hold_note," +
                    $"clmdxcode01,clmdxcode02,clmdxcode03,clmdxcode04,clmdxcode05,clmdxcode06,clmdxcode07,clmdxcode08,clmdxcode09,clmdxcode10,clmdxcode11,clmdxcode12,IssueDxCode01,IssueCategory01," +
                    $"IssueDxCode02,IssueCategory02,IssueDxCode03,IssueCategory03,MemberList,NONHCC_ADD_DX01,NONHCC_ADD_DX02,NONHCC_ADD_DX03,NONHCC_ADD_DX04,NONHCC_REM_DX01,NONHCC_REM_DX02,NONHCC_REM_DX03," +
                    $"NONHCC_REM_DX04,[patient firstname],[patient lastname],[ptnt gm mrn],[ptnt grnwy mrn],patientid,[ptnt lgcy ptnt id],[patient primary ins pkg id],[patient primary ins pkg name]," +
                    $"[patient primary ins pkg type],[patient primary policyidnumber],[primary Claimstatus],proccode,[rndrng prvdrfullnme],reviewdate,reviewnote,reviewer,[rndrng prvdrid],[ptnt op mrn]," +
                    $"RptCat_ChargeErrorCorrected,RptCat_HCC_Add,RptCat_HCC_Add_Count,RptCat_HCC_Add_New,RptCat_HCC_Add_Specificity,RptCat_HCC_NoChange,RptCat_HCC_Remove,RptCat_HCC_Remove_Count,RptCat_Hold," +
                    $"RptCat_NonHCC_Change,RptCat_PreviouslyReviewed,[secondary Claimstatus],StartTime,[trnsctn postdate],[trnsctn pstvd],[trnsctn srvdate],[trnsctn type],[trnsctn voideddate],[trnsctn vdd ysn]," +
                    $"[transactivtyid],[wklst assgnd usr],[wklst escalated],[wklst name],[wklst note],[wklst status date],[wklst team name],agree,notes)" +
                    $" VALUES ('{entry.ChargeID}','{entry.ChargePostDate}','{entry.ChargePriority}','{entry.ClaimHCC_ESRD_Count}','{entry.ClaimHCC_V2x_Count}','{entry.ClaimID}','{entry.ClaimPriority}'," +
                    $"'{entry.ClaimStatus}','{entry.ClmsvcdeptID}','{entry.Contract}','{entry.CstmInsGrpng}','{entry.CurrCstmRule}','{entry.DaysInHoldStatus}','{entry.DemandBilled}','{entry.DepartmentBillingName}'," +
                    $"'{entry.FileDate}','{entry.HCC_ADD_DX01}','{entry.HCC_ADD_DX01_Reason}','{entry.HCC_ADD_DX02}','{entry.HCC_ADD_DX02_Reason}','{entry.HCC_ADD_DX03}','{entry.HCC_ADD_DX03_Reason}','{entry.HCC_ADD_DX04}','{entry.HCC_ADD_DX04_Reason}'," +
                    $"'{entry.HCC_ADD_DX05}','{entry.HCC_ADD_DX05_Reason}','{entry.HCC_ADD_DX06}','{entry.HCC_ADD_DX06_Reason}','{entry.HCC_ADD_DX07}','{entry.HCC_ADD_DX07_Reason}'," +
                    $"'{entry.HCC_ADD_DX08}','{entry.HCC_ADD_DX08_Reason}','{entry.HCC_ADD_DX09}','{entry.HCC_ADD_DX09_Reason}','{entry.HCC_ADD_DX10}','{entry.HCC_ADD_DX10_Reason}','{entry.HCC_ADD_DX11}','{entry.HCC_ADD_DX11_Reason}'," +
                    $"'{entry.HCC_ADD_DX12}','{entry.HCC_ADD_DX12_Reason}','{entry.HCC_REM_DX01}','{entry.HCC_REM_DX02}','{entry.HCC_REM_DX03}','{entry.HCC_REM_DX04}','{entry.Hold}','{entry.Hold_Reason}','{entry.Hold_Note}'," +
                    $"'{entry.ICD10ClaimDiagCode01}','{entry.ICD10ClaimDiagCode02}','{entry.ICD10ClaimDiagCode03}','{entry.ICD10ClaimDiagCode04}','{entry.ICD10ClaimDiagCode05}','{entry.ICD10ClaimDiagCode06}'," +
                    $"'{entry.ICD10ClaimDiagCode07}','{entry.ICD10ClaimDiagCode08}','{entry.ICD10ClaimDiagCode09}','{entry.ICD10ClaimDiagCode10}','{entry.ICD10ClaimDiagCode11}','{entry.ICD10ClaimDiagCode12}'," +
                    $"'{entry.IssueDXCode01}','{entry.IssueCategory01}','{entry.IssueDXCode02}','{entry.IssueCategory02}','{entry.IssueDXCode03}','{entry.IssueCategory03}','{entry.MemberList}'," +
                    $"'{entry.NonHCC_ADD_DX01}','{entry.NonHCC_ADD_DX02}','{entry.NonHCC_ADD_DX03}','{entry.NonHCC_ADD_DX04}','{entry.NonHCC_REM_DX01}','{entry.NonHCC_REM_DX02}','{entry.NonHCC_REM_DX03}'," +
                    $"'{entry.NonHCC_REM_DX04}','{entry.PatientFirstName}','{entry.PatientLastName}','{entry.PatientGMMRN}','{entry.PatientGreenwayMRN}','{entry.PatientID}','{entry.PatientLegacyPatientID}','{entry.PatientPrimaryInsPkgID}'," +
                    $"'{entry.PatientPrimaryInsPkgName}','{entry.PatientPrimaryInsPkgType}','{entry.PatientPrimaryPolicyIDNumber}','{entry.PrimaryClaimstatus}','{entry.ProcCode}','{entry.ProviderFullName}'," +
                    $"'{entry.ReviewDate}','{entry.ReviewNote}','{entry.Reviewer}','{entry.RndrngProviderID}','{entry.PatientOPMRN}','{entry.RptCat_ChargeErrorCorrected}','{entry.RptCat_HCC_Add}'," +
                    $"'{entry.RptCat_HCC_Add_Count}','{entry.RptCat_HCC_Add_New}','{entry.RptCat_HCC_Add_Specificity}','{entry.RptCat_HCC_NoChange}','{entry.RptCat_HCC_Remove}','{entry.RptCat_HCC_Remove_Count}'," +
                    $"'{entry.RptCat_Hold}','{entry.RptCat_NonHCC_Change}','{entry.Rptcat_PreviouslyReviewed}','{entry.SecondaryClaimstatus}','{entry.StartTime}','{entry.TransactionPostDate}'," +
                    $"'{entry.TransactionPstvd}','{entry.TransactionSrvDate}','{entry.TransactionType}','{entry.TransactionVoidedDate}','{entry.TransactionVddYsn}','{entry.TransactivityID}'," +
                    $"'{entry.WklstAssignedUser}','{entry.WklstEscalated}','{entry.WklstName}','{entry.WklstNote}','{entry.WklstStatusDate}','{entry.WklstTeamName}','{decision}','{note}')";

            }
            //if decision is disagreed 
            else if (decision == false)
            {
                decision = true;
                sql = $"INSERT INTO HCC_Completed_Athena_Worklist (chargeid,chrpostdate,ChargePriority,ClaimHCC_ESRD_Count,ClaimHCC_V2x_Count,claimid,ClaimPriority,claimstatus,clmsvcdeptid,[contract],[cstm ins grpng]" +
                     $",[curr cstm rule],[days in hold status],DemandBilled,[svc dept bill name],FileDate,HCC_ADD_DX01,HCC_ADD_DX01_Reason,HCC_ADD_DX02,HCC_ADD_DX02_Reason,HCC_ADD_DX03,HCC_ADD_DX03_Reason," +
                     $"HCC_ADD_DX04,HCC_ADD_DX04_Reason,HCC_ADD_DX05,HCC_ADD_DX05_Reason,HCC_ADD_DX06,HCC_ADD_DX06_Reason,HCC_ADD_DX07,HCC_ADD_DX07_Reason,HCC_ADD_DX08,HCC_ADD_DX08_Reason,HCC_ADD_DX09,HCC_ADD_DX09_Reason," +
                     $"HCC_ADD_DX10,HCC_ADD_DX10_Reason,HCC_ADD_DX11,HCC_ADD_DX11_Reason,HCC_ADD_DX12,HCC_ADD_DX12_Reason,HCC_REM_DX01,HCC_REM_DX02,HCC_REM_DX03,HCC_REM_DX04,hold,hold_reason,hold_note," +
                     $"clmdxcode01,clmdxcode02,clmdxcode03,clmdxcode04,clmdxcode05,clmdxcode06,clmdxcode07,clmdxcode08,clmdxcode09,clmdxcode10,clmdxcode11,clmdxcode12,IssueDxCode01,IssueCategory01," +
                     $"IssueDxCode02,IssueCategory02,IssueDxCode03,IssueCategory03,MemberList,NONHCC_ADD_DX01,NONHCC_ADD_DX02,NONHCC_ADD_DX03,NONHCC_ADD_DX04,NONHCC_REM_DX01,NONHCC_REM_DX02,NONHCC_REM_DX03," +
                     $"NONHCC_REM_DX04,[patient firstname],[patient lastname],[ptnt gm mrn],[ptnt grnwy mrn],patientid,[ptnt lgcy ptnt id],[patient primary ins pkg id],[patient primary ins pkg name]," +
                     $"[patient primary ins pkg type],[patient primary policyidnumber],[primary Claimstatus],proccode,[rndrng prvdrfullnme],reviewdate,reviewnote,reviewer,[rndrng prvdrid],[ptnt op mrn]," +
                     $"RptCat_ChargeErrorCorrected,RptCat_HCC_Add,RptCat_HCC_Add_Count,RptCat_HCC_Add_New,RptCat_HCC_Add_Specificity,RptCat_HCC_NoChange,RptCat_HCC_Remove,RptCat_HCC_Remove_Count,RptCat_Hold," +
                     $"RptCat_NonHCC_Change,RptCat_PreviouslyReviewed,[secondary Claimstatus],StartTime,[trnsctn postdate],[trnsctn pstvd],[trnsctn srvdate],[trnsctn type],[trnsctn voideddate],[trnsctn vdd ysn]," +
                     $"[transactivtyid],[wklst assgnd usr],[wklst escalated],[wklst name],[wklst note],[wklst status date],[wklst team name],disagree,notes)" +
                     $" VALUES ('{entry.ChargeID}','{entry.ChargePostDate}','{entry.ChargePriority}','{entry.ClaimHCC_ESRD_Count}','{entry.ClaimHCC_V2x_Count}','{entry.ClaimID}','{entry.ClaimPriority}'," +
                     $"'{entry.ClaimStatus}','{entry.ClmsvcdeptID}','{entry.Contract}','{entry.CstmInsGrpng}','{entry.CurrCstmRule}','{entry.DaysInHoldStatus}','{entry.DemandBilled}','{entry.DepartmentBillingName}'," +
                     $"'{entry.FileDate}','{entry.HCC_ADD_DX01}','{entry.HCC_ADD_DX01_Reason}','{entry.HCC_ADD_DX02}','{entry.HCC_ADD_DX02_Reason}','{entry.HCC_ADD_DX03}','{entry.HCC_ADD_DX03_Reason}','{entry.HCC_ADD_DX04}','{entry.HCC_ADD_DX04_Reason}'," +
                     $"'{entry.HCC_ADD_DX05}','{entry.HCC_ADD_DX05_Reason}','{entry.HCC_ADD_DX06}','{entry.HCC_ADD_DX06_Reason}','{entry.HCC_ADD_DX07}','{entry.HCC_ADD_DX07_Reason}'," +
                     $"'{entry.HCC_ADD_DX08}','{entry.HCC_ADD_DX08_Reason}','{entry.HCC_ADD_DX09}','{entry.HCC_ADD_DX09_Reason}','{entry.HCC_ADD_DX10}','{entry.HCC_ADD_DX10_Reason}','{entry.HCC_ADD_DX11}','{entry.HCC_ADD_DX11_Reason}'," +
                     $"'{entry.HCC_ADD_DX12}','{entry.HCC_ADD_DX12_Reason}','{entry.HCC_REM_DX01}','{entry.HCC_REM_DX02}','{entry.HCC_REM_DX03}','{entry.HCC_REM_DX04}','{entry.Hold}','{entry.Hold_Reason}','{entry.Hold_Note}'," +
                     $"'{entry.ICD10ClaimDiagCode01}','{entry.ICD10ClaimDiagCode02}','{entry.ICD10ClaimDiagCode03}','{entry.ICD10ClaimDiagCode04}','{entry.ICD10ClaimDiagCode05}','{entry.ICD10ClaimDiagCode06}'," +
                     $"'{entry.ICD10ClaimDiagCode07}','{entry.ICD10ClaimDiagCode08}','{entry.ICD10ClaimDiagCode09}','{entry.ICD10ClaimDiagCode10}','{entry.ICD10ClaimDiagCode11}','{entry.ICD10ClaimDiagCode12}'," +
                     $"'{entry.IssueDXCode01}','{entry.IssueCategory01}','{entry.IssueDXCode02}','{entry.IssueCategory02}','{entry.IssueDXCode03}','{entry.IssueCategory03}','{entry.MemberList}'," +
                     $"'{entry.NonHCC_ADD_DX01}','{entry.NonHCC_ADD_DX02}','{entry.NonHCC_ADD_DX03}','{entry.NonHCC_ADD_DX04}','{entry.NonHCC_REM_DX01}','{entry.NonHCC_REM_DX02}','{entry.NonHCC_REM_DX03}'," +
                     $"'{entry.NonHCC_REM_DX04}','{entry.PatientFirstName}','{entry.PatientLastName}','{entry.PatientGMMRN}','{entry.PatientGreenwayMRN}','{entry.PatientID}','{entry.PatientLegacyPatientID}','{entry.PatientPrimaryInsPkgID}'," +
                     $"'{entry.PatientPrimaryInsPkgName}','{entry.PatientPrimaryInsPkgType}','{entry.PatientPrimaryPolicyIDNumber}','{entry.PrimaryClaimstatus}','{entry.ProcCode}','{entry.ProviderFullName}'," +
                     $"'{entry.ReviewDate}','{entry.ReviewNote}','{entry.Reviewer}','{entry.RndrngProviderID}','{entry.PatientOPMRN}','{entry.RptCat_ChargeErrorCorrected}','{entry.RptCat_HCC_Add}'," +
                     $"'{entry.RptCat_HCC_Add_Count}','{entry.RptCat_HCC_Add_New}','{entry.RptCat_HCC_Add_Specificity}','{entry.RptCat_HCC_NoChange}','{entry.RptCat_HCC_Remove}','{entry.RptCat_HCC_Remove_Count}'," +
                     $"'{entry.RptCat_Hold}','{entry.RptCat_NonHCC_Change}','{entry.Rptcat_PreviouslyReviewed}','{entry.SecondaryClaimstatus}','{entry.StartTime}','{entry.TransactionPostDate}'," +
                     $"'{entry.TransactionPstvd}','{entry.TransactionSrvDate}','{entry.TransactionType}','{entry.TransactionVoidedDate}','{entry.TransactionVddYsn}','{entry.TransactivityID}'," +
                     $"'{entry.WklstAssignedUser}','{entry.WklstEscalated}','{entry.WklstName}','{entry.WklstNote}','{entry.WklstStatusDate}','{entry.WklstTeamName}','{decision}','{note}')";
            }

            string insert = sql;


            DbQuery query = new DbQuery();
            string connection = query.AccessString;
            SqlConnection conn = new SqlConnection(connection);
            SqlCommand insertCommand = new SqlCommand();
            insertCommand = new SqlCommand(insert, conn);
            conn.Open();
            int l = insertCommand.ExecuteNonQuery();
            conn.Close();



        }
        public async Task<List<WorklistEntry>> SelectCompletedReview(WorklistEntry model)
        {

            Task<List<WorklistEntry>> result = Task.Run(async () =>
            {
                DbQuery query = new DbQuery();
                DateTime beginningDate = DateTime.Now.AddDays(-7);
                string sql = $"SELECT * FROM  HCC_Completed_Athena_Worklist WHERE claimid = '{model.ClaimID}'";
                List<WorklistEntry> taskResult = await query.ExecuteAsync<WorklistEntry>(sql);
                return taskResult;
            });
            List<WorklistEntry> filteredResults = await FilterCompletedClaimsByPriority(result);
            return filteredResults;


        }

        public async Task<List<WorklistEntry>> PeerReviewManagers(string filedate, string contract)
        {


            DateTime SelectedDate = new DateTime(1900, 1, 1);
            DateTime.TryParse(filedate, out SelectedDate);

            // Should come out in the format yyyyMMdd.
            string dateStringForSqlConvert =
                $"{SelectedDate.Year.ToString()}{SelectedDate.Month.ToString().PadLeft(2, '0')}{SelectedDate.Day.ToString().PadLeft(2, '0')}";

            Task<List<WorklistEntry>> result = Task.Run(async () =>
            {
                DbQuery query = new DbQuery();
                DateTime beginningDate = DateTime.Now.AddDays(-7);
                string sql =
                    $"SELECT t1.chargeid, t1.patientid, t1.claimid, t1.[patient lastname] AS PatientLastName, t1.[patient firstname] AS PatientFirstName, " +
                    $"t1.[trnsctn srvdate] AS TransactionSrvDate, t1.proccode, t1.FileDate, t1.Contract, t1.MemberList, t1.ClaimPriority, t1.Reviewer, t1.ReviewDate, t1.Contract, t1.StartTime, t1.Hold_Reason,  +" +
                    $" t1.ChargePriority FROM HCC_Completed_Athena_Worklist AS t1 WHERE Contract = '{contract}' AND FileDate > dateadd(dd, -7, Convert(DateTime, '{dateStringForSqlConvert}', 112)) and FileDate <= Convert(DateTime, '{dateStringForSqlConvert}', 112) AND manager_completed IS NULL ORDER BY ChargePriority Asc";
                List<WorklistEntry> taskResult = await query.ExecuteAsync<WorklistEntry>(sql);
                return taskResult;
            });
            List<WorklistEntry> filteredResults = await FilterCompletedClaimsByPriority(result);
            return filteredResults;



        }

        public async void UpdateCompletedReview(WorklistEntry entry, bool decision, string sql,string note)
        {
            if (decision == true)
            {
                sql = $"UPDATE HCC_Completed_Athena_Worklist SET agree = 'True', disagree = NULL , manager_completed = 'True', notes = '{note}' WHERE claimid = '{entry.ClaimID}' ";

            }
            //if decision is disagreed 
            else if (decision == false)
            {
                sql = $"UPDATE HCC_Completed_Athena_Worklist SET agree = NULL, disagree = 'True', manager_completed = 'True', notes = '{note}' WHERE claimid = '{entry.ClaimID}' ";
            }

            string insert = sql;


            DbQuery query = new DbQuery();
            string connection = query.AccessString;
            SqlConnection conn = new SqlConnection(connection);
            SqlCommand insertCommand = new SqlCommand();
            insertCommand = new SqlCommand(insert, conn);
            conn.Open();
            int l = insertCommand.ExecuteNonQuery();
            conn.Close();
        }

        public List<string> GetContractMenuItemsCompleted()
        {
            string sql = "SELECT DISTINCT Contract FROM HCC_Athena_Worklist WHERE contract not in ('NextGenACO','CPC Plus','NoContract')";
            DbQuery query = new DbQuery();
            List<string> result = query.Execute<string>(sql);
            return result.OrderBy(q => q).ToList();
        }

        public List<ContractCount> GetContractCountsConpleted(string date = "")
        {

            if (string.IsNullOrEmpty(date))
                date = DateTime.Now.Date.ToShortDateString();
            List<ContractCount> returnValues = new List<ContractCount>();
            List<string> contracts = GetContractMenuItems();
            foreach (string s in contracts)
            {
                string sql = $"SELECT COUNT(Contract) FROM {tableName} WHERE Contract = '{s}' AND FileDate = '{DataUtilities.ConvertToSqlFormattedDateTime(date)}' AND Reviewer IS NOT NULL AND ReviewDate IS NOT NULL";
                DbQuery query = new DbQuery();
                returnValues.Add(new ContractCount()
                {
                    ContractName = s,
                    Count = query.ExecuteSingle<int>(sql)

                });
            }
            return returnValues;
        }

        public async Task<List<WorklistEntry>> SelectBoolsFromDatabase(HccWorkEntryViewModel model)
        {

            Task<List<WorklistEntry>> result = Task.Run(async () =>
            {
                DbQuery query = new DbQuery();
                DateTime beginningDate = DateTime.Now.AddDays(-7);
                string sql = $"SELECT * FROM  HCC_Completed_Athena_Worklist WHERE claimid = '{model.Entry.ClaimID}'";
                List<WorklistEntry> taskResult = await query.ExecuteAsync<WorklistEntry>(sql);
                return taskResult;
            });
            List<WorklistEntry> filteredResults = await FilterCompletedClaimsByPriority(result);
            return filteredResults;


        }


        public WorklistEntry DecisionSetUp(WorklistEntry model)
        {
            string sql = $"SELECT  * FROM HCC_Completed_Athena_Worklist WHERE claimid = '{model.ClaimID}'";
            DbQuery query = new DbQuery();
            string connection = query.AccessString;
            SqlConnection conn = new SqlConnection(connection);
            SqlCommand insertCommand = new SqlCommand(sql, conn);
            conn.Open();
            var reader = insertCommand.ExecuteReader();

            while (reader.Read())
            {
                string agree = reader["agree"].ToString();
                string disagree = reader["disagree"].ToString();

                if (agree != "")
                {
                    model.agree = true;
                }
                if (disagree != "")
                {
                    model.disagree = true;
                }

            }


            conn.Close();



            return model;
        }

        public async Task<List<WorklistEntry>> SelectAllPeerReviews()
        {

            DateTime SelectedDate = new DateTime(1900, 1, 1);
            DateTime.TryParse(DateTime.Now.ToString(), out SelectedDate);

            // Should come out in the format yyyyMMdd.
            string dateStringForSqlConvert =
                $"{SelectedDate.Year.ToString()}{SelectedDate.Month.ToString().PadLeft(2, '0')}{SelectedDate.Day.ToString().PadLeft(2, '0')}";

            Task<List<WorklistEntry>> result = Task.Run(async () =>
            {
                DbQuery query = new DbQuery();
                DateTime beginningDate = DateTime.Now.AddDays(-7);
                string sql =
                    $"SELECT t1.chargeid, t1.patientid, t1.claimid, t1.[patient lastname] AS PatientLastName, t1.[patient firstname] AS PatientFirstName, " +
                    $"t1.[trnsctn srvdate] AS TransactionSrvDate, t1.proccode, t1.FileDate, t1.Contract, t1.MemberList, t1.ClaimPriority, t1.Reviewer, t1.ReviewDate, t1.Contract, t1.StartTime, t1.Hold_Reason,  +" +
                    $" t1.ChargePriority FROM HCC_Completed_Athena_Worklist AS t1 WHERE FileDate > dateadd(dd, -90, Convert(DateTime, '{dateStringForSqlConvert}', 112)) and FileDate <= Convert(DateTime, '{dateStringForSqlConvert}', 112) ORDER BY ChargePriority Asc";
                List<WorklistEntry> taskResult = await query.ExecuteAsync<WorklistEntry>(sql);
                return taskResult;
            });
            List<WorklistEntry> filteredResults = await FilterCompletedClaimsByPriority(result);
            return filteredResults;


        }

        public async Task<WorklistEntry> GetWorklistEntryAsyncCompleted(string chargeid)
        {
            //Note:  chargeid is INT in the database, however, because this field value is basically coming from the user form, it is returned as string 
            //from the view.  SQL server implicitly converts it during query execution.
            string sql =
            $"SELECT * FROM HCC_Completed_Athena_Worklist WHERE chargeid = '{chargeid}'";
            DbQuery query = new DbQuery();
            return await query.ExecuteSingleAsync<WorklistEntry>(sql);
        }

        public async Task<WorklistEntry> GetWorklistEntryandLockAsyncCompleted(string chargeid, string username)
        {


            GlobalUserLogic userLogic = new GlobalUserLogic();

            WorklistEntry entry = await GetWorklistEntryAsyncCompleted(chargeid);
            if (IsClaimLocked(entry, username))
            {
                HccRecordLock lockRecord = GetActiveLockRecord(entry.ClaimID);
                User u = userLogic.GetUserByUsername(lockRecord.LockedBy);
                entry.LockedBy = u;
                //If the requesting user is the one who locked it, it should not be read only
                entry.IsReadOnly = lockRecord.LockedBy != username;
                entry.LockId = lockRecord.Id;
                if (entry.Reviewer == null)
                    entry.Reviewer = $"{u.LastName}, {u.FirstName}";
                return entry;
            }
            else
            {
                string sql =
                    $"INSERT INTO HccRecordLocks (ClaimId, LockDateTime, LockedBy) VALUES ('{entry.ClaimID}', '{DateTime.Now}', '{username}') SELECT @@IDENTITY";
                DbQuery query = new DbQuery();

                entry.LockId = await query.ExecuteScalarAsync<HccRecordLock>(sql);
                entry.LockedBy = userLogic.GetUserByUsername(username);
                entry.IsReadOnly = false;
                return entry;
            }
        }

    }
}
