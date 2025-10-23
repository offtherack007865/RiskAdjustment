using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskAdjustment.Data.Models;
using RiskAdjustment.Data.Models.HccWorkflow;
using RiskAdjustment.Data.Models.RefTables;

namespace RiskAdjustment.Data.ViewModels.HCC
{
    public class HccWorkEntryViewModel
    {
        public HccWorkEntryViewModel()
        {
            IssueCategories = new List<string>()
            {

                "Dx in Assess-not PN",
                "Dx in PN-not Assess",
                "New HCC Opportunity",
                "Incomplete Note",
                "99499 needed",
                "Missing PE",
                "Clarification",
                ""
            };

            HccAddReasons = new List<string>()
            {
                "New",
                "Specificity"
            };
        }

        public WorklistEntry Entry { get; set; }
        public List<WorklistEntry> RelatedCharges { get; set; }
        public User CurrentUser { get; set; }
        public List<ICD10Entry> ICD10Entries { get; set; }

        // PWM - 20240312 - INC0124213
        // The Json string was getting too long for the Json.Encode method in the cshtml,
        // so I am doing the encoding on the server placing the JSON string in the model.
        public string ICD10EntriesJsonString { get; set; }
        public List<PreviousWorkNote> PreviousNotes { get; set; }
        public List<string> IssueCategories { get; set; }
        public List<string> HccAddReasons { get; set; }
        public List<User> ActiveUsers { get; set; }


    }
}
