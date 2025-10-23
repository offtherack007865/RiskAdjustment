using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskAdjustment.Data.Models.PvpWorkflow;
using RiskAdjustment.Data.Models.RefTables;

namespace RiskAdjustment.Data.ViewModels.PVP
{
    public class PvpWorkEntryViewModel
    {
        public PvpWorkEntryViewModel()
        {
            PatientCaseReasons = new List<string>()
            {
                "Reason 1",
                "Reason 2",
                "Reason 3"
            };
        }
        public PvpWorklistEntry Entry { get; set; }
        public User CurrentUser { get; set; }
        public List<ICD10Entry> ICD10Entries { get; set; }

        // pwm - 20240313 - INC0124213
        // The Json string was getting too long for the Json.Encode method in the cshtml,
        // so I am doing the encoding it on the server and placing it in the model.
        public string ICD10EntriesJsonString { get; set; }
        public List<User> ActiveUsers { get; set; }
        public List<string> PatientCaseReasons { get; set; }


    }
}
