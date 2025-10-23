using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskAdjustment.z.DbSeeder
{
    class Program
    {
        static void Main(string[] args)
        {
            //UserWorker worker = new UserWorker();
            //worker.MakeDbTables();
            //worker.SeedTable();

            //SiteWorker siteWorker = new SiteWorker();
            //siteWorker.MakeDbTables();
            //siteWorker.SeedDbTables();

            //HccRecordLockWorker lockWorker = new HccRecordLockWorker();
            //lockWorker.MakeDbTable();

            //HccWorker hccWorker = new HccWorker();
            //hccWorker.Seed();



            //Uncomment and run this to install PVP

            PvpRecordLockWorker pvpLockWorker = new PvpRecordLockWorker();
            pvpLockWorker.MakeTable();

            PvpAuditSeeder pvpseeder = new PvpAuditSeeder();
            pvpseeder.Seed();

        }
    }
}
