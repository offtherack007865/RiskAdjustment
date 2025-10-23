using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskAdjustment.Data.Models;
using RiskAdjustment.Data.Models.RefTables;

namespace RiskAdjustment.Data.ViewModels.HCC
{
    public class HccWorklistViewModel
    {
        public List<WorklistEntry> WorklistEntries { get; set; }
        public string FileDate { get; set; }
        public string Contract { get; set; }
        public User CurrentUser { get; set; }

    }
}
