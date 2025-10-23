using RiskAdjustment.Data.Models;
using RiskAdjustment.Data.Models.RefTables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskAdjustment.Data.ViewModels.HCC
{
    public class HccReviewsPendingViewModel
    {
        public List<WorklistEntry> WorklistEntries { get; set; }
        public User CurrentUser { get; set; }
    }
}
