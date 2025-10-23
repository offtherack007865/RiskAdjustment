using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskAdjustment.Data.Models.PvpWorkflow;
using RiskAdjustment.Data.Models.RefTables;

namespace RiskAdjustment.Data.ViewModels.PVP
{
    public class PvpWorklistViewModel
    {
        public List<PvpWorklistEntry> WorklistEntries { get; set; }
        public string FileDate { get; set; }
        public string Contract { get; set; }
        public User CurrentUser { get; set; }

        // pwm - 20240525 - Add Priority
        public int Priority { get; set; }
    }
}
