using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using RiskAdjustment.Data;
using RiskAdjustment.Data.Attributes;
using RiskAdjustment.Data.Models.HccWorkflow;
using RiskAdjustment.Data.Models.PvpWorkflow;
using RiskAdjustment.Data.Models.RefTables;
using RiskAdjustment.Data.ViewModels;
using RiskAdjustment.Logic.Global;


namespace RiskAdjustment.Logic.Pvp
{
    public class PvpLogic : IPvpLogic
    {
        private readonly string tableName = "Athena_PVP_Worklist";

        private Lazy<GlobalUserLogic> _userLogic;

        public PvpLogic()
        {
            this._userLogic = new Lazy<GlobalUserLogic>();
        }

        public List<string> GetPvpContractMenuItems()
        {
            string sql = $"SELECT DISTINCT Contract FROM {tableName} WHERE contract not in ('Medicare','Next_Gen')";
            DbQuery query = new DbQuery(DbType.PVP);
            List<string> result = query.Execute<string>(sql);
            return result.OrderBy(q => q).ToList();
        }

        public List<ContractCount> GetContractCounts(string date = "")
        {
            if (string.IsNullOrEmpty(date))
                date = DateTime.Now.Date.ToShortDateString();
            List<ContractCount> returnValues = new List<ContractCount>();
            List<string> contracts = GetPvpContractMenuItems();

            foreach (string s in contracts)
            {
                string sql = $"SELECT COUNT(Contract) FROM {tableName} WHERE Contract = '{s}' AND FileDate = '{DataUtilities.ConvertToSqlFormattedDateTime(date)}'";
                DbQuery query = new DbQuery(DbType.PVP);
                returnValues.Add(new ContractCount()
                {
                    ContractName = s,
                    Count = query.ExecuteSingle<int>(sql)

                });
            }
            return returnValues;
        }

        public async Task<List<PvpWorklistEntry>> GetUnlockedWorklistEntriesForDateAsync(string date, string contract)
        {
            string convertedDate = DataUtilities.ConvertToSqlFormattedDateTime(date);

            // pwm - 20240525 - Add priority to the list and do initial sort by it.
            // pwm - 20250414 - Omit entries listing the following providers.
            /*                  Lori Austin - AUSTIN_LORI
                                Amy Bentley - BENTLEY_AMY
                                Glen Hall - HALL_GLEN
                                Eric Lisic - LISIC_ERIC
                                Angela Meyer - MEYER_ANGELA
                                Jaime Oakley - OAKLEY_JAIME
                                Charles Sharpe - SHARPE_CHARLES
                                Jonathan Smeltzer - SMELTZER_JONATHAN
                                Bhavana Vora - VORA_BHAVANA
                                Clyde Worley - WORLEY_CLYDE

             */

            string omitProviderListString = $"AND AppointmentProvider NOT IN ('AUSTIN_LORI', 'BENTLEY_AMY', 'HALL_GLEN', 'LISIC_ERIC', 'MEYER_ANGELA', 'OAKLEY_JAIME', 'SHARPE_CHARLES', 'SMELTZER_JONATHAN', 'VORA_BHAVANA', 'WORLEY_CLYDE')";

            string sql =
                $"SELECT AppointmentID, PatientName, PatientID, PVPApptDate,PcpSiteName,PcpName,AppointmentTypeShortName, Priority FROM  Athena_PVP_Worklist WHERE FileDate = '{convertedDate}' AND contract = '{contract}' AND ReviewDate IS NULL {omitProviderListString} order by Priority asc";
            DbQuery query = new DbQuery(DbType.PVP);


            string locksSql = @"SELECT * FROM PvpRecordLocks WHERE DATEDIFF(dd, LockDateTime, CURRENT_TIMESTAMP) < 8";
            List<PvpRecordLock> recordLocks = query.Execute<PvpRecordLock>(locksSql);

            return await RemoveLockedEntries(await query.ExecuteAsync<PvpWorklistEntry>(sql), recordLocks);
        }

        public async Task<PvpWorklistEntry> GetWorklistEntryandLockAsync(string appointmentId, string username)
        {
            GlobalUserLogic userLogic = new GlobalUserLogic();
            PvpWorklistEntry entry = await GetWorklistEntryAsync(appointmentId);
            if (IsRecordLocked(entry, username))
            {
                PvpRecordLock lockRecord = GetActiveRecordLock(appointmentId);
                User u = userLogic.GetUserByUsername(lockRecord.LockedBy);
                entry.LockedBy = u;
                //If the requesting user is the one who locked it, it should not be read only
                entry.IsReadOnly = lockRecord.LockedBy != username;
                entry.LockId = lockRecord.Id;
                entry.Reviewer = $"{u.LastName}, {u.FirstName}";

                return entry;
            }
            else
            {
                string sql =
                    $"INSERT INTO PvpRecordLocks (AppointmentID, LockDateTime, LockedBy) VALUES ('{entry.AppointmentID}', '{DateTime.Now}', '{username}') SELECT @@IDENTITY";
                DbQuery query = new DbQuery(DbType.PVP);

                entry.LockId = await query.ExecuteScalarAsync<HccRecordLock>(sql);
                entry.LockedBy = userLogic.GetUserByUsername(username);
                entry.IsReadOnly = false;
                return entry;
            }
        }

        public async Task<PvpWorklistEntry> GetWorklistEntryAsync(string appointmentId)
        {
            string sql = $"SELECT * FROM Athena_PVP_Worklist WHERE AppointmentID = {appointmentId}";

            // pwm - 20250303 - Get Provider Names to Highlight.
            string sqlForHighlightedProviderNames =
                $"SELECT ProviderName FROM PvpProviderNameToHighlight";
            DbQuery query = new DbQuery(DbType.PVP);
            List<PvpProviderNameToHighlight> 
                providerNameToHighlightRecordsList = 
                query
                .Execute<PvpProviderNameToHighlight>(sqlForHighlightedProviderNames);

            query = new DbQuery(DbType.PVP);
            Task<PvpWorklistEntry> 
                returnPvpWorkListEntryTask =
                    query.ExecuteSingleAsync<PvpWorklistEntry>(sql);

            List<PvpProviderNameToHighlight> 
                pvpProviderNameToHighlightRecordsList =
                    providerNameToHighlightRecordsList
                    .Where(p => p.ProviderName.CompareTo(returnPvpWorkListEntryTask.Result.AppointmentProvider) == 0)
                    .ToList();

            // if AppointmentProvider field matches one of
            // the provider names to be highlighted.
            if (pvpProviderNameToHighlightRecordsList.Count > 0)
            {
                returnPvpWorkListEntryTask.Result.HighlightedProvider =
                    true;
            }
            else
            {
                List<PvpProviderNameToHighlight> 
                pvpProviderNameToHighlightRecordsList2 =
                    providerNameToHighlightRecordsList
                    .Where(p => p.ProviderName.CompareTo(returnPvpWorkListEntryTask.Result.PcpName) == 0)
                    .ToList();

                // if PCPName field matches one of
                // the provider names to be highlighted.
                if (pvpProviderNameToHighlightRecordsList2.Count > 0)
                {
                    returnPvpWorkListEntryTask.Result.HighlightedProvider =
                        true;
                }
                else
                {
                    returnPvpWorkListEntryTask.Result.HighlightedProvider =
                        false;
                }
            }
            
            return await returnPvpWorkListEntryTask;
        }

        // pwm - 20240525 - Priority 
        public async Task<List<PvpWorklistEntry>> GetPvpReviewsInProgressAsync()
        {
            Task<List<PvpWorklistEntry>> resultsTask = Task.Run(async () =>
            {
                List<PvpWorklistEntry> results = new List<PvpWorklistEntry>();
                string lockSql = "SELECT * FROM PvpRecordLocks WHERE LockReleasedBy IS NULL AND LockReleaseDateTime IS NULL";                
                DbQuery query = new DbQuery(DbType.PVP);

                List<PvpRecordLock> pvpLocks = await query.ExecuteAsync<PvpRecordLock>(lockSql);
                foreach(PvpRecordLock pvplock in pvpLocks)
                {
                    User u = this._userLogic.Value.GetUserByUsername(pvplock.LockedBy);
                    string sql = $"SELECT * FROM Athena_PVP_WorkList WHERE AppointmentID = {pvplock.AppointmentID} ORDER BY Priority";
                    PvpWorklistEntry entry = await query.ExecuteSingleAsync<PvpWorklistEntry>(sql);
                    entry.Reviewer = entry.Reviewer ?? $"{u.LastName}, {u.FirstName}";
                    entry.StartDate = pvplock.LockDateTime.Date;
                    results.Add(entry);
                }
                return results;
            });
            await Task.WhenAll(resultsTask);
            return resultsTask.Result;
        }
        // pwm - 20240525 - Priority 
        public async Task<List<PvpWorklistEntry>> GetCompleted90DaysAsync()
        {
            Task<List<PvpWorklistEntry>> result = Task.Run(async () =>
            {
                List<PvpWorklistEntry> taskResult = new List<PvpWorklistEntry>();
                DbQuery query = new DbQuery(DbType.PVP);
                DateTime beginningDate = DateTime.Now.AddDays(-45);
                string sql = $"SELECT AppointmentID, PatientName, PatientID, PVPApptDate, Contract, Reviewer, StartTime, ReviewDate, Priority " +
                $"FROM Athena_PVP_Worklist WHERE Reviewer IS NOT NULL AND ReviewDate IS NOT NULL AND ReviewDate > CAST('{beginningDate}' AS DATE) ORDER BY Priority";
                List<PvpWorklistEntry> entries = await query.ExecuteAsync<PvpWorklistEntry>(sql);
                foreach(PvpWorklistEntry entry in entries)
                {
                    if(!IsRecordLocked(entry.AppointmentID))
                        taskResult.Add(entry);
                }

                return taskResult;
            });
            await Task.WhenAll(result);
            return result.Result;
        }

        public async void ClearAllFieldsAsync(PvpWorklistEntry e, string username)
        {
            List<string> excludedProperties = new List<string>()
            {
                "AppointmentID, AppointmentDateTime"
            };
            _ = Task.Run(async () =>
            {
                foreach (PropertyInfo property in e.GetType().GetProperties())
                {
                    var t = typeof(PvpWorklistEntry);
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

        public async Task<string> GetHccGapsThisYear(int patientId)
        {
            string timeFrame = "TY";
            return await GetHccGapsForYear(patientId, timeFrame);            
        }

        public async Task<string> GetHccGapsLastYear(int patientId)
        {
            string timeFrame = "LY";
            return await GetHccGapsForYear(patientId, timeFrame);
        }
        

        public async Task<int> UpdateAsync(PvpWorklistEntry entry, string auditUserName)
        {
            if (entry.DismissedDate == new DateTime(1,1,1))
            {
                entry.DismissedDate = new DateTime(1900,1,1);
            }
            if (entry.StartDate == new DateTime(1,1,1))
            {
                entry.StartDate = new DateTime(1900,1,1);
            }

            string sql = $@"UPDATE Athena_PVP_WorkList 
                SET Reviewer = @Reviewer, 
                ReviewDate = @ReviewDate,
                ReviewNote = @ReviewNote,
                StartTime = @StartTime,
                HCC01Code = @HCC01Code,
                HCC02Code = @HCC02Code,
                HCC03Code = @HCC03Code,
                HCC04Code = @HCC04Code,
                HCC05Code = @HCC05Code,
                HCC06Code = @HCC06Code,
                HCC07Code = @HCC07Code,
                HCC08Code = @HCC08Code,
                HCC09Code = @HCC09Code,
                HCC10Code = @HCC10Code,
                HCC01 = @HCC01,
                HCC02 = @HCC02,
                HCC03 = @HCC03,
                HCC04 = @HCC04,
                HCC05 = @HCC05,
                HCC06 = @HCC06,
                HCC07 = @HCC07,
                HCC08 = @HCC08,
                HCC09 = @HCC09,
                HCC10 = @HCC10,
                OpenGapHCC01Code = @OpenGapHCC01Code,
                OpenGapHCC02Code = @OpenGapHCC02Code,
                OpenGapHCC03Code = @OpenGapHCC03Code,
                OpenGapHCC04Code = @OpenGapHCC04Code,
                OpenGapHCC05Code = @OpenGapHCC05Code,
                OpenGapHCC06Code = @OpenGapHCC06Code,
                OpenGapHCC07Code = @OpenGapHCC07Code,
                OpenGapHCC08Code = @OpenGapHCC08Code,
                OpenGapHCC09Code = @OpenGapHCC09Code,
                OpenGapHCC10Code = @OpenGapHCC10Code,
                OpenGapHCC01 = @OpenGapHCC01,
                OpenGapHCC02 = @OpenGapHCC02,
                OpenGapHCC03 = @OpenGapHCC03,
                OpenGapHCC04 = @OpenGapHCC04,
                OpenGapHCC05 = @OpenGapHCC05,
                OpenGapHCC06 = @OpenGapHCC06,
                OpenGapHCC07 = @OpenGapHCC07,
                OpenGapHCC08 = @OpenGapHCC08,
                OpenGapHCC09 = @OpenGapHCC09,
                OpenGapHCC10 = @OpenGapHCC10,
                SuspectHCC01Code = @SuspectHCC01Code,
                SuspectHCC02Code = @SuspectHCC02Code,
                SuspectHCC03Code = @SuspectHCC03Code,
                SuspectHCC04Code = @SuspectHCC04Code,
                SuspectHCC05Code = @SuspectHCC05Code,
                SuspectHCC01 = @SuspectHCC01,
                SuspectHCC02 = @SuspectHCC02,
                SuspectHCC03 = @SuspectHCC03,
                SuspectHCC04 = @SuspectHCC04,
                SuspectHCC05 = @SuspectHCC05,
                SuspectHCC06 = @SuspectHCC06,
                SuspectHCC07 = @SuspectHCC07,
                SuspectHCC08 = @SuspectHCC08,
                CPT_99499_ICD = @CPT_99499_ICD,
                CPT_99499_DOS = @CPT_99499_DOS,
                PVPNoteAdded = @PVPNoteAdded,
                PortalReview = @PortalReview,
                ProblemListUpdate = @ProblemListUpdate,
                EmailNotification = @EmailNotification,
                BannerAlertAdded = @BannerAlertAdded,
                PatientCaseSent = @PatientCaseSent,
                PatientCaseDate = @PatientCaseDate,
                PatientCaseCode01 = @PatientCaseCode01,
                PatientCaseCode02 = @PatientCaseCode02,
                PatientCaseCode03 = @PatientCaseCode03,
                PatientCaseCode04 = @PatientCaseCode04,
                PatientCaseCode05 = @PatientCaseCode05,
                PatientCaseHCC01 = @PatientCaseHCC01,
                PatientCaseHCC02 = @PatientCaseHCC02,
                PatientCaseHCC03 = @PatientCaseHCC03,
                PatientCaseHCC04 = @PatientCaseHCC04,
                PatientCaseHCC05 = @PatientCaseHCC05,
                PatientCaseHCCReplaced01 = @PatientCaseHCCReplaced01,
                PatientCaseHCCReplaced02 = @PatientCaseHCCReplaced02,
                PatientCaseHCCReplaced03 = @PatientCaseHCCReplaced03,
                PatientCaseHCCReplaced04 = @PatientCaseHCCReplaced04,
                PatientCaseHCCReplaced05 = @PatientCaseHCCReplaced05,
                Dismissed = @Dismissed,
		        DismissedDate = @DismissedDate,
		        DismissedHCC01Code = @DismissedHCC01Code,
		        DismissedHCC01 = @DismissedHCC01,
                DismissedHCC02Code = @DismissedHCC02Code,
	            DismissedHCC02 = @DismissedHCC02,
                DismissedHCC03Code = @DismissedHCC03Code,
	            DismissedHCC03 = @DismissedHCC03,
                DismissedHCC04Code = @DismissedHCC04Code,
	            DismissedHCC04 = @DismissedHCC04,
                DismissedHCC05Code = @DismissedHCC05Code,
	            DismissedHCC05 = @DismissedHCC05Code,
		        CorrectedClaims = @CorrectedClaims,
		        CorrectedClaimsNumber = @CorrectedClaimsNumber,
	            CorrectedClaimsComments = @CorrectedClaimsComments 

                WHERE AppointmentID = @AppointmentID";

            DbQuery query = new DbQuery(DbType.PVP);
            query.ActionExecuted += new EventHandler<DbQueryEventArgs>((sender, e) => AuditPvpAction(sender, e, entry, DbAction.Update, auditUserName));
            return await query.ExecuteScalarAsync(sql, entry);
        }

        public async void ClearUserLockForAppointmentAsync(string username, int AppointmentID)
        {
            await Task.Run(() =>
            {
                string sql = $"UPDATE PvpRecordLocks SET LockReleaseDateTime = '{DateTime.Now}', LockReleasedBy = '{username} (auto)' WHERE LockedBy = '{username}' AND AppointmentID = { AppointmentID }";
                DbQuery query = new DbQuery(DbType.PVP);
                query.ExecuteCommandAsync(sql);
                return Task.CompletedTask;
            });
        }
        public async void ClearUserLockForAppointmentAsync(int lockId, string username)
        {
            _ = Task.Run(() =>
            {
                string sql = $"UPDATE PvpRecordLocks SET LockReleaseDateTime = '{DateTime.Now}', LockReleasedBy = '{username} (auto)', Pending = 0 WHERE Id = {lockId}";
                DbQuery query = new DbQuery(DbType.PVP);
                query.ExecuteCommandAsync(sql);

            });
            await Task.CompletedTask;
        }

        private async void AuditPvpAction(object sender, DbQueryEventArgs e, PvpWorklistEntry entry, DbAction action, string auditUsername)
        {
            string sql = "INSERT INTO Audit_Athena_PVP_Worklist (AppointmentID, StartTime, HCC01Code, HCC02Code, HCC03Code, HCC04Code, HCC05Code, HCC01, HCC02, " +
                "HCC03, HCC04, HCC05, HCC6plus, PortalReview, PVPNoteAdded, NoNoteReason, Reviewer, ReviewDate, Note, CPT_99499_ICD, CPT_99499_DOS, PreviousPVPDate, " +
                "ArrivedApptDate, Rework_Reviewer, Rework_Date, Rework_HCCProblems, Rework_HCCAddressed, EmailNotification, BannerAlertAdded, patdecdate, AuditDate, Auditor, " +
                "AuditResult, AuditNote, PatientCaseSent, ReworkNote, Rework99429_ICD, Rework99429_Date, DxSubmittedOnClaim, ProblemListUpdate, PatientCaseDate, PatientCaseCode01, PatientCaseCode02, " +
                "PatientCaseCode03, PatientCaseCode04, PatientCaseCode05, PatientCaseHCC01, PatientCaseHCC02, PatientCaseHCC03, PatientCaseHCC04, PatientCaseHCC05, " +
                "PatientCaseHCCReplaced01, PatientCaseHCCReplaced02, PatientCaseHCCReplaced03, PatientCaseHCCReplaced04, PatientCaseHCCREplaced05, AuditId, AuditUser, AuditAction, AuditDateTime, " +
                 "HCC09Code, HCC09, HCC10Code, HCC10, OpenGapHCC01Code, OpenGapHCC01, OpenGapHCC02Code, OpenGapHCC02, OpenGapHCC03Code, OpenGapHCC03, OpenGapHCC04Code, OpenGapHCC04, " +
                 "OpenGapHCC05Code, OpenGapHCC05, OpenGapHCC06Code, OpenGapHCC06, OpenGapHCC07Code, OpenGapHCC07, OpenGapHCC08Code, OpenGapHCC08, OpenGapHCC09Code, OpenGapHCC09, " +
                 "OpenGapHCC10Code, OpenGapHCC10, Dismissed, DismissedDate, DismissedHCC01Code, DismissedHCC01, DismissedHCC02Code, DismissedHCC02, DismissedHCC03Code, DismissedHCC03, " +
                 "DismissedHCC04Code, DismissedHCC04, DismissedHCC05Code, DismissedHCC05, CorrectedClaims, CorrectedClaimsNumber, CorrectedClaimsComments) " +
            "VALUES (@AppointmentID, @StartTime, @HCC01Code, @HCC02Code, @HCC03Code, @HCC04Code, @HCC05Code, @HCC01, @HCC02, @HCC03, @HCC04, @HCC05, " +
                "@HCC6plus, @PortalReview, @PVPNoteAdded, @NoNoteReason, @Reviewer, @ReviewDate, @ReviewNote, @CPT_99499_ICD, @CPT_99499_DOS, @PreviousPVPDate, " +
                "@ArrivedApptDate, @Rework_Reviewer, @Rework_Date, @Rework_HCCProblems, @Rework_HCCAddressed, @EmailNotification, @BannerAlertAdded, @patdecdate, @AuditDate, @Auditor, " +
                "@AuditResult, @AuditNote, @PatientCaseSent, @ReworkNote, @Rework99429_ICD, @Rework99429_Date, @DxSubmittedOnClaim, @ProblemListUpdate, @PatientCaseDate, @PatientCaseCode01, @PatientCaseCode02, " +
                "@PatientCaseCode03, @PatientCaseCode04, @PatientCaseCode05, @PatientCaseHCC01, @PatientCaseHCC02, @PatientCaseHCC03, @PatientCaseHCC04, @PatientCaseHCC05, " +
                $"@PatientCaseHCCReplaced01, @PatientCaseHCCReplaced02, @PatientCaseHCCReplaced03, @PatientCaseHCCReplaced04, @PatientCaseHCCReplaced05, @AppointmentID, '{auditUsername}', '{DbAction.Update}', '{DateTime.Now}', " +
                 "@HCC09Code, @HCC09, @HCC10Code, @HCC10, @OpenGapHCC01Code, @OpenGapHCC01, @OpenGapHCC02Code, @OpenGapHCC02, @OpenGapHCC03Code, @OpenGapHCC03, @OpenGapHCC04Code, @OpenGapHCC04, " +
                 "@OpenGapHCC05Code, @OpenGapHCC05, @OpenGapHCC06Code, @OpenGapHCC06, @OpenGapHCC07Code, @OpenGapHCC07, @OpenGapHCC08Code, @OpenGapHCC08, @OpenGapHCC09Code, @OpenGapHCC09, " +
                 "@OpenGapHCC10Code, @OpenGapHCC10, @Dismissed, @DismissedDate, @DismissedHCC01Code, @DismissedHCC01, @DismissedHCC02Code, @DismissedHCC02, @DismissedHCC03Code, @DismissedHCC03, " +
                 "@DismissedHCC04Code, @DismissedHCC04, @DismissedHCC05Code, @DismissedHCC05, @CorrectedClaims, @CorrectedClaimsNumber, @CorrectedClaimsComments) ";

            _ = Task.Run(async () =>
            {
                DbQuery query = new DbQuery(DbType.PVP);
                return await query.ExecuteScalarAsync(sql, entry);
            });
            await Task.CompletedTask;
        }

        private async Task<string> GetHccGapsForYear(int patientId, string timeFrame)
        {
            Task<string> resultsTask = Task.Run(async () =>
            {
                DbQuery query = new DbQuery(DbType.PVP);
                string sql = $"SELECT HCC_w_Description FROM HealthPlan_Member_HCC_Status WHERE PatientID = { patientId } AND LY_TY = '{ timeFrame }' ";
                List<string> results = await query.ExecuteAsync<string>(sql);
                StringBuilder sb = new StringBuilder();
                foreach (string s in results)
                    sb.AppendLine(s);
                return sb.ToString();
            });
            await Task.WhenAll(resultsTask);
            return resultsTask.Result;
        }

        private bool IsRecordLocked(PvpWorklistEntry recordId, string username)
        {
            string locksSql = @"SELECT * FROM PvpRecordLocks WHERE DATEDIFF(dd, LockDateTime, CURRENT_TIMESTAMP) < 30";
            DbQuery query = new DbQuery(DbType.PVP);
            List<PvpRecordLock> locks = query.Execute<PvpRecordLock>(locksSql);
            return IsRecordLocked(recordId, locks);
        }

        private bool IsRecordLocked(int AppointmentID)
        {
            string sql = $"SELECT * FROM PvpRecordLocks WHERE AppointmentId = {AppointmentID}";
            DbQuery query = new DbQuery(DbType.PVP);
            List<PvpRecordLock> locks = query.Execute<PvpRecordLock>(sql);
            foreach(PvpRecordLock recordLock in locks)
            {
                if (recordLock.LockReleaseDateTime == null)
                    return true;
            }
            return false;
        }

        private bool IsRecordLocked(PvpWorklistEntry entry, List<PvpRecordLock> locks)
        {
            PvpRecordLock checkLock = locks.FirstOrDefault(x => (x.AppointmentID == entry.AppointmentID.ToString()) && x.LockReleaseDateTime == null);
            if (checkLock != null)
                return true;
            return false;
        }

        private PvpRecordLock GetActiveRecordLock(string AppointmentID)
        {
            string sql = $"SELECT * FROM PvpRecordLocks WHERE AppointmentID = '{AppointmentID}' AND LockReleaseDateTime IS NULL";
            DbQuery query = new DbQuery(DbType.PVP);
            return query.ExecuteSingle<PvpRecordLock>(sql);
        }

        private async Task<List<PvpWorklistEntry>> RemoveLockedEntries(List<PvpWorklistEntry> entries, List<PvpRecordLock> locks)
        {
            Task<List<PvpWorklistEntry>> resultsTask = Task.Run(() =>
            {
                List<PvpWorklistEntry> results = new List<PvpWorklistEntry>();
                foreach (PvpWorklistEntry entry in entries)
                {
                    if (!IsRecordLocked(entry, locks))
                    {
                        results.Add(entry);
                    }   
                }

                return results;
            });

            await Task.WhenAll(resultsTask);
            return resultsTask.Result;
        }
    }
}
