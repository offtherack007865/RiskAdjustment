using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskAdjustment.Data.Models;
using RiskAdjustment.Data.Models.HccWorkflow;
using RiskAdjustment.Data.ViewModels;
using RiskAdjustment.Data.ViewModels.HCC;

namespace RiskAdjustment.Logic.Hcc
{
    public interface IHccLogic
    {
        /// <summary>
        /// Retrieves a unique, enumerated list of all contract names
        /// </summary>
        /// <returns></returns>
        List<string> GetContractMenuItems();

        /// <summary>
        /// Counts the number of Worklist entries for each contract that have yet to be reviewed.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        List<ContractCount> GetContractCounts(string date = "");

        /// <summary>
        /// Gets the un-reviewed worklist entries for the date and contract specified
        /// </summary>
        /// <param name="date"></param>
        /// <param name="contract"></param>
        /// <returns></returns>
        Task<List<WorklistEntry>> GetUnlockedWorklistEntriesForDateAsync(string date, string contract);

        /// <summary>
        /// Asynchronously examines ICD-10 diagnoses fields in all worklist items and assigns them priority based on diagnostic criteria
        /// </summary>
        /// <param name="entries">List of Worklist Entries</param>
        /// <returns>Priority assigned and sorted list of Worklist Entries</returns>
        Task<List<WorklistEntry>> SetWorklistPriorityAsync(Task<List<WorklistEntry>> entries);

        /// <summary>
        /// Asynchronously gets a worklist entry from the database
        /// </summary>
        /// <param name="chargeid">Charge Id of the worklist item</param>
        /// <returns>Worklist entry</returns>
        Task<WorklistEntry> GetWorklistEntryAsync(string chargeid);

        /// <summary>
        /// Asynchronously gets a worklist item and locks the record from other users
        /// </summary>
        /// <param name="chargeid">Charge Id of the worklist item</param>
        /// <param name="username">Username of the currently logged in user.  Assigns the lock to this user.</param>
        /// <returns></returns>
        Task<WorklistEntry> GetWorklistEntryandLockAsync(string chargeid, string username);

        /// <summary>
        /// Gets the highest priority charge for a claim
        /// </summary>
        /// <param name="claimId">Id of the claim</param>
        /// <returns>worklist entry</returns>
        Task<WorklistEntry> GetHighestPriorityItemForClaim(int claimId);

        /// <summary>
        /// Gets any other CPTs (charges) for the given claim id
        /// </summary>
        /// <param name="claimid"></param>
        /// <returns></returns>
        Task<List<WorklistEntry>> GetRelatedCptsAsync(int claimid, int currentChargeId, int patientId, DateTime? dateOfService, string placeOfService);


        /// <summary>
        /// Clears the lock record for a specific charge ID and username
        /// </summary>
        /// <param name="username">username who holds the lock</param>
        /// <param name="chargeid">Charge ID</param>
        void ClearUserLockForCharge(string username, string chargeid);

        void ClearUserLockForCharge(int lockId, string username);

        //Locks a previously unlocked lock record
        void LockRecord(int lockid);

        /// <summary>
        /// Gets any work notes for the patient in the database for the current year
        /// </summary>
        /// <param name="patientId"></param>
        /// <returns></returns>
        Task<List<PreviousWorkNote>> GetPtPreviousWorkNotesAsync(int patientId);

        /// <summary>
        /// Asynchronously clears all record locks of the current user.
        /// </summary>
        /// <param name="userName"></param>
        void ClearAllRecordLocksForUserAsync(string userName);

        /// <summary>
        /// Sets priority for the worklist entry based on ICD-10 diagnostic criteria
        /// </summary>
        /// <param name="w">Worklist entry</param>
        void SetPriority(WorklistEntry w);

        void ClearAllFieldsAsync(WorklistEntry e, string username);

        /// <summary>
        /// Updates the Worklist entry
        /// </summary>
        /// <param name="entry">Worklist Entry</param>
        /// <param name="auditUserName">HCC Coding specialist username</param>
        /// <returns></returns>
        Task<int> UpdateAsync(WorklistEntry entry, string auditUserName);

        /// <summary>
        /// Re-locks the HCCRecordLock and flags the charge as a pending review
        /// </summary>
        /// <param name="lockId"></param>
        void AddToPending(int lockId);

        Task<List<WorklistEntry>> GetReviewsInProgressAsync();

        //Task<List<WorklistEntry>> GetReviewsPendedAsync();
        //Task<List<WorklistEntry>> GetReviewsEscalatedAsync();

        Task<List<WorklistEntry>> GetCompleted90DaysAsync();

        Task<List<WorklistEntry>> PeerReviewCompleted(string filedate, string contract);

        Task<List<WorklistEntry>> selectFromDatabase(WorklistEntry model);

        void InsertToCompletedDatabase(WorklistEntry entry, bool decision, string sql, string note);

        Task<List<WorklistEntry>> PeerReviewManagers(string filedate, string contract);

        Task<List<WorklistEntry>> SelectCompletedReview(WorklistEntry model);

        void UpdateCompletedReview(WorklistEntry entry, bool decision, string sql,string note);

        List<ContractCount> GetContractCountsConpleted(string date = "");

        List<string> GetContractMenuItemsCompleted();

        Task<List<WorklistEntry>> SelectBoolsFromDatabase(HccWorkEntryViewModel model);

        WorklistEntry DecisionSetUp(WorklistEntry model);

        Task<List<WorklistEntry>> SelectAllPeerReviews();

        Task<WorklistEntry> GetWorklistEntryAsyncCompleted(string chargeid);
        Task<WorklistEntry> GetWorklistEntryandLockAsyncCompleted(string chargeid, string username);



    }
}
