using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskAdjustment.Data;

namespace RiskAdjustment.z.DbSeeder
{
    public class HccRecordLockWorker
    {

        public void MakeDbTable()
        {
            string mainTbl = "CREATE TABLE HccRecordLocks (" +
                         "Id INTEGER IDENTITY(1,1)," +
                         "ClaimId NVARCHAR(50)," +
                         "LockDateTime DATETIME," +
                         "LockedBy NVARCHAR(100)," +
                         "LockReleaseDateTime DATETIME," +
                         "LockReleasedBy NVARCHAR(100)," +
                         "Pending BIT)";

            DbQuery query = new DbQuery();
            query.ExecuteCommand(mainTbl);
        }
    }
}
