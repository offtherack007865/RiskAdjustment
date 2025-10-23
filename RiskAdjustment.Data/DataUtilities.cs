using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskAdjustment.Data
{
    public static class DataUtilities
    {
        public static string ConvertToSqlFormattedDateTime(string dateTime)
        {
            DateTime sourceDt = DateTime.Parse(dateTime);
            return sourceDt.ToString("yyyy-MM-dd 00:00:00.000");
        }
    }
}
