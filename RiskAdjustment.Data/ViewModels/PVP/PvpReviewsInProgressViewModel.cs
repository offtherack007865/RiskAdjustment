using RiskAdjustment.Data.Models.PvpWorkflow;
using RiskAdjustment.Data.Models.RefTables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskAdjustment.Data.ViewModels.PVP
{
    public class PvpReviewsInProgressViewModel
    {
        public List<PvpWorklistEntry> WorklistEntries { get; set; }
        public User CurrentUser { get; set; }

    }
}
