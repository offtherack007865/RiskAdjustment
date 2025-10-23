using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskAdjustment.Data.Models.PvpWorkflow
{
    public class PvpRecordLock
    {
        public int Id { get; set; }
        public string AppointmentID { get; set; }
        public DateTime LockDateTime { get; set; }
        public string LockedBy { get; set; }
        public DateTime? LockReleaseDateTime { get; set; }
        public string LockReleasedBy { get; set; }
        public bool Pending { get; set; }
    }
}
