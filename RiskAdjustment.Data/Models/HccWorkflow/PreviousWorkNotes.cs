using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskAdjustment.Data.Models.HccWorkflow
{
    public class PreviousWorkNote
    {
        public DateTime FileDate { get; set; }
        public int ClaimID { get; set; }

        //TODO:  Find this field
        public string WklstNote { get; set; }
    }
}
