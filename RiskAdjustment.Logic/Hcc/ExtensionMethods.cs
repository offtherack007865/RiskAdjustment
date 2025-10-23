using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskAdjustment.Data;
using RiskAdjustment.Data.Models;
using RiskAdjustment.Data.Models.RefTables;

namespace RiskAdjustment.Logic.Hcc
{
    public static class ExtensionMethods
    {
        //public static bool IsManagerOrAdmin(this User user)
        //{
        //    if (user.Role == "Manager" || user.Role == "Admin")
        //        return true;
        //    return false;
        //}

        public static bool IsChildEntry(this WorklistEntry entry)
        {
            DbQuery query = new DbQuery();
            string sql = $"SELECT COUNT(claimid) FROM HCC_Athena_Worklist WHERE claimid = {entry.ClaimID} AND ChargePriority < '{entry.ChargePriority}'";
            int result = query.ExecuteSingle<int>(sql);
            if (result > 0)
                return true;
            else
                return false;
        }
    }
}
