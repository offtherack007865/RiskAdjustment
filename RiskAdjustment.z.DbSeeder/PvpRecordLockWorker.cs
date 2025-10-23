using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskAdjustment.Data;

namespace RiskAdjustment.z.DbSeeder
{
    public class PvpRecordLockWorker
    {
        public void MakeTable()
        {
            string mainTbl = "CREATE TABLE PvpRecordLocks (" +
                             "Id INTEGER IDENTITY(1,1)," +
                             "AppointmentId NVARCHAR(50)," +
                             "LockDateTime DATETIME," +
                             "LockedBy NVARCHAR(100)," +
                             "LockReleaseDateTime DATETIME," +
                             "LockReleasedBy NVARCHAR(100)," +
                             "Pending BIT)";

            DbQuery query = new DbQuery(DbType.PVP);
            query.ExecuteCommand(mainTbl);
        }
    }
}
