using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskAdjustment.Data.Models.HccWorkflow
{
    public class HccRecordLock
    {
        public int Id { get; set; }
        public string ClaimID { get; set; }
        public DateTime LockDateTime { get; set; }
        public string LockedBy { get; set; }
        public DateTime? LockReleaseDateTime { get; set; }
        public string LockReleasedBy { get; set; }
        public bool Pending { get; set; }
    }
}
