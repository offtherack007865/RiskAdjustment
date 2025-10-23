using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskAdjustment.Data;

namespace RiskAdjustment.z.DbSeeder
{
    public class UserWorker
    {
        public void MakeDbTables()
        {
            string mainTbl = "CREATE TABLE Users (" +
                         "Id INTEGER IDENTITY(1,1), " +
                         "Initials NVARCHAR(10), " +
                         "UserName NVARCHAR(25), " +
                         "FirstName NVARCHAR(30), " +
                         "LastName NVARCHAR(30), " +
                         "Email NVARCHAR(50)," + 
                         "ActiveDate NVARCHAR(50), " + 
                         "InactiveDate NVARCHAR(50), " +
                         "Note NVARCHAR(255), " +
                         "AthenaId NVARCHAR(15), " +
                         "Contract NVARCHAR(100), " +
                         "Role NVARCHAR(100)" + ")";

            string auditTbl = "CREATE TABLE Audit_Users (" +
                              "Id INTEGER IDENTITY(1,1), " +
                              "Initials NVARCHAR(10), " +
                              "UserName NVARCHAR(25), " +
                              "FirstName NVARCHAR(30), " +
                              "LastName NVARCHAR(30), " +
                              "Email NVARCHAR(50)," +
                              "ActiveDate NVARCHAR(50), " +
                              "InactiveDate NVARCHAR(50), " +
                              "Note NVARCHAR(255), " +
                              "AthenaId NVARCHAR(15), " +
                              "Contract NVARCHAR(100), " +
                              "Role NVARCHAR(100)," +
                              "AuditId INTEGER," +
                              "AuditUser NVARCHAR(50)," +
                              "AuditAction NVARCHAR(50)," +
                              "AuditDate DATETIME" +
                                ")";


            DbQuery query = new DbQuery();
            query.ExecuteCommand(mainTbl);
            query.ExecuteCommand(auditTbl);
        }

        public void SeedTable()
        {
            List<string> statements = new List<string>()
            {
                
                "INSERT INTO Users (Initials, UserName, FirstName, LastName, Email, ActiveDate, Note, AthenaId, Contract, Role) VALUES ('JAL', 'jalavan', 'Jessica', 'Lavan', 'jalavan@summithealthcare.com', '3/23/2018', 'test 123', '1234455', 'All', 'Manager')",
                "INSERT INTO Users (Initials, UserName, FirstName, LastName, Email, ActiveDate, Note, AthenaId, Contract, Role) VALUES ('BRL', 'blefler', 'Brad', 'Lefler', 'blefler@summithealthcare.com', '1/1/2018', 'brad', '13579', 'All', 'Admin')",
                "INSERT INTO Users (Initials, UserName, FirstName, LastName, Email, ActiveDate, Note, AthenaId, Contract, Role) VALUES ('JRH', 'jrhaun', 'Julie', 'Haun', 'jrhaun@summithealthcare.com', '1/10/2019', '', '', 'All', 'Coding Support Specialist')",
                "INSERT INTO Users (Initials, UserName, FirstName, LastName, Email, ActiveDate, Note, AthenaId, Contract, Role) VALUES ('JAR', 'jarodgers', 'Jason', 'Rodgers', 'jarodgers@summithealthcare.com', '2/5/2018', '', '', 'All', 'Coding Support Specialist')",
                "INSERT INTO Users (Initials, UserName, FirstName, LastName, Email, ActiveDate, Note, AthenaId, Contract, Role) VALUES ('JAG', 'jagately', 'Jamie', 'Gately', 'jagately@summithealthcare.com', '1/10/2019', '', '', 'All', 'Coding Support Specialist')",
                "INSERT INTO Users (Initials, UserName, FirstName, LastName, Email, ActiveDate, Note, AthenaId, Contract, Role) VALUES ('MCM', 'mcmalicoat', 'Melissa', 'Malicoat', 'mcmalicoat@summithealthcare.com', '2/7/2017', '', '', 'All', 'Coding Support Specialist')",
                "INSERT INTO Users (Initials, UserName, FirstName, LastName, Email, ActiveDate, Note, AthenaId, Contract, Role) VALUES ('AAS', 'aasparkes', 'Amy', 'Sparkes', 'aasparkes@summithealthcare.com', '3/26/2018', '', '', 'All', 'Coding Support Specialist')",
                "INSERT INTO Users (Initials, UserName, FirstName, LastName, Email, ActiveDate, Note, AthenaId, Contract, Role) VALUES ('BLM', 'blmickinnis', 'Brittany', 'McKinnis', 'blmickinnis@summithealthcare.com', '1/10/2019', '', '', 'All', 'Manager')",
                "INSERT INTO Users (Initials, UserName, FirstName, LastName, Email, ActiveDate, Note, AthenaId, Contract, Role) VALUES ('KAM', 'kameyer', 'Kellie', 'Meyer', 'kameyer@summithealthcare.com', '5/22/2018', '', '', 'All', 'Coding Support Specialist')",
                "INSERT INTO Users (Initials, UserName, FirstName, LastName, Email, ActiveDate, Note, AthenaId, Contract, Role) VALUES ('MDM', 'mdmorgan', 'Melanie', 'Morgan', 'mdmorgan@summithealthcare.com', '5/22/2018', '', '', 'All', 'Coding Support Specialist')",
                "INSERT INTO Users (Initials, UserName, FirstName, LastName, Email, ActiveDate, Note, AthenaId, Contract, Role) VALUES ('LAH', 'laheatherly', 'Laura', 'Heatherly', 'laheatherly@summithealthcare.com', '10/21/2018', '', '', 'All', 'Coding Support Specialist')",
                "INSERT INTO Users (Initials, UserName, FirstName, LastName, Email, ActiveDate, Note, AthenaId, Contract, Role) VALUES ('CRB', 'crbailey', 'Colleen', 'Bailey', 'crbailey@summithealthcare.com', '1/20/2020', '', '', 'All', 'Coding Support Specialist')",
                "INSERT INTO Users (Initials, UserName, FirstName, LastName, Email, ActiveDate, Note, AthenaId, Contract, Role) VALUES ('KBG', 'kbgrosenback', 'Kerrie', 'Gosenback', 'kbgrosenback@summithealthcare.com', '3/5/2020', '', '', 'All', 'Coding Support Specialist')",
                "INSERT INTO Users (Initials, UserName, FirstName, LastName, Email, ActiveDate, Note, AthenaId, Contract, Role) VALUES ('SLK', 'slkelly', 'Samantha', 'Kelly', 'slkelly@summithealthcare.com', '6/1/2020', '', '', 'All', 'Coding Support Specialist')",
                "INSERT INTO Users (Initials, UserName, FirstName, LastName, Email, ActiveDate, Note, AthenaId, Contract, Role) VALUES ('SFG', 'sjgardner', 'Susan', 'Gardner', 'sfgardner@summithealthcare.com', '6/1/2020', '', '', 'All', 'Coding Support Specialist')",
                "INSERT INTO Users (Initials, UserName, FirstName, LastName, Email, ActiveDate, Note, AthenaId, Contract, Role) VALUES ('NSC', 'nscoffey', 'Nick', 'Coffey', 'nscoffey@summithealthcare.com', '1/1/2018', 'nick', '123456', 'All', 'Admin')",
                "INSERT INTO Users (Initials, UserName, FirstName, LastName, Email, ActiveDate, Note, AthenaId, Contract, Role) VALUES ('CEB', 'cbridgman', 'Carol', 'Bridgman', 'cbridgman@summithealthcare.com', '1/1/208', 'carol', '123456', 'All', 'Admin')"

            };

            foreach (string s in statements)
            {
                DbQuery query = new DbQuery();
                query.ExecuteCommand(s);
            }
        }
    }
}
