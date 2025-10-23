using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using RiskAdjustment.Data.Models.PvpWorkflow;
using RiskAdjustment.Data.ViewModels;

namespace RiskAdjustment.Logic.Pvp
{
    public interface IPvpLogic
    {
        /// <summary>
        /// Gets all unique contracts present in the PVP database
        /// </summary>
        /// <returns></returns>
        List<string> GetPvpContractMenuItems();

        /// <summary>
        /// Counts the number of occurances of each unique contract value from the PVP database
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        List<ContractCount> GetContractCounts(string date = "");

         /// <summary>
         /// Gets uncompleted worklist entries that are unlocked from the PVP database
         /// </summary>
         /// <param name="date">Contract date from the PVP select list screen</param>
         /// <param name="contract">contract value from the PVP select list screen</param>
         /// <returns></returns>
        Task<List<PvpWorklistEntry>> GetUnlockedWorklistEntriesForDateAsync(string date, string contract);

         /// <summary>
         /// Gets a PVP worklist entry from the PVP database and generates a new PVP lock file.
         /// </summary>
         /// <param name="appointmentId"></param>
         /// <param name="username"></param>
         /// <returns></returns>
        Task<PvpWorklistEntry>GetWorklistEntryandLockAsync(string appointmentId, string username);

        /// <summary>
        /// Gets a PVP worklist entry from the PVP database
        /// </summary>
        /// <param name="appointmentId"></param>
        /// <returns></returns>
        Task<PvpWorklistEntry> GetWorklistEntryAsync(string appointmentId);

        /// <summary>
        /// Gets reviews in progress from the PVP database
        /// </summary>
        /// <returns></returns>
        Task<List<PvpWorklistEntry>> GetPvpReviewsInProgressAsync();


        /// <summary>
        /// Gets completed reviews from the last 90 days
        /// </summary>
        /// <returns></returns>
        Task<List<PvpWorklistEntry>> GetCompleted90DaysAsync();

        /// <summary>
        /// Retrieves the patients' HCC Gaps for the current year
        /// </summary>
        /// <param name="patiendId"></param>
        /// <returns></returns>
        Task<string> GetHccGapsThisYear(int patiendId);

        /// <summary>
        /// Retrieves the patients' HCC Gaps for the previous year
        /// </summary>
        /// <param name="patientId"></param>
        /// <returns></returns>
        Task<string> GetHccGapsLastYear(int patientId);

        Task<int> UpdateAsync(PvpWorklistEntry entry, string auditUserName);

        void ClearUserLockForAppointmentAsync(int apptId, string username);

        void ClearAllFieldsAsync(PvpWorklistEntry e, string username);

        void ClearUserLockForAppointmentAsync(string username, int AppointmentID);
    }
}
