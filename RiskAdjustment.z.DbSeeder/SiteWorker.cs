using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskAdjustment.Data;

namespace RiskAdjustment.z.DbSeeder
{
    public class SiteWorker
    {
        public void MakeDbTables()
        {
            string mainTbl = "CREATE TABLE Sites (" +
                             "Id INTEGER IDENTITY(1,1), " +
                             "SiteNumber NVARCHAR(10), " +
                             "Name NVARCHAR(100), " +
                             "County NVARCHAR(30), " +
                             "Active BIT )";

            string auditTbl = "CREATE TABLE Audit_Sites (" +
                              "Id INTEGER IDENTITY(1,1), " +
                              "SiteNumber NVARCHAR(10), " +
                              "Name NVARCHAR(100), " +
                              "County NVARCHAR(30), " +
                              "Active BIT, " +
                              "AuditId INTEGER," +
                              "AuditUser NVARCHAR(50)," +
                              "AuditAction NVARCHAR(50)," +
                              "AuditDate DATETIME" +
                              ")";

            DbQuery query = new DbQuery();
            query.ExecuteCommand(mainTbl);
            query.ExecuteCommand(auditTbl);
        }

        public void SeedDbTables()
        {
            List<string> statements = new List<string>()
            {
                "INSERT INTO Sites (SiteNumber, Name, County, Active) VALUES ('PCP131', 'Norwood Family Medicine', 'Knox', 1)",
                "INSERT INTO Sites (SiteNumber, Name, County, Active) VALUES ('PCP110', 'Concord Medical Center', 'Knox', 1)"

            };
            foreach (string s in statements)
            {
                DbQuery query = new DbQuery();
                query.ExecuteCommand(s);
            }
        }
    }
}
