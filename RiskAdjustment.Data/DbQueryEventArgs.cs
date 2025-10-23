using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskAdjustment.Data
{
    public class DbQueryEventArgs : EventArgs
    {
        public object Entity { get; set; }
        public int RecordId { get; set; }
        public string Message { get; set; }
    }
}
