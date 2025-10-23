using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskAdjustment.Data.ViewModels.HCC
{
    public class HccMenuViewModel
    {
        public List<string> MenuValues { get; set; }
        public List<ContractCount> ContractCounts { get; set; }
        public DateTime CountsDate { get; set; }
    }
}
