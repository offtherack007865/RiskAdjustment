using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using RiskAdjustment.Data;
using RiskAdjustment.Data.Models.RefTables;
using Dapper;

namespace RiskAdjustment.z.UnityTests.Unit
{
    [TestFixture]
    public class DbLogicTests
    {
        private string connectionString =
            "Data Source = vs-dev-db; Initial Catalog = HCC_CodingReview; Persist Security Info=True;User ID =  hccconversion_user; Password=Summit99$";
        //[TestCase()]
        //public void DbInsertOrUpdateSqlAppendateForAuditing()
        //{
        //    User user = new User()
        //    {
        //        Initials = "GGG",
        //        FirstName = "Db",
        //        LastName = "Test",
        //        Email = "test@summithealthcare.com",
        //        Note = "test",
        //        AthenaId = "1234",
        //        Contract = "All",
        //        Role = "Manager"

        //    };
        //    string sql = $"INSERT INTO Users (Initials, FirstName, LastName, Email, Note, AthenaId, Contract, Role) VALUES ('{user.Initials}', '{user.FirstName}', '{user.LastName}', '{user.Email}', '{user.Note}', '{user.AthenaId}', '{user.Contract}', '{user.Role}');";
        //    DbQuery query = new DbQuery();
        //    //string newSql = query.AuditEntity(sql, typeof(User));
        //    using (SqlConnection connection = new SqlConnection(this.connectionString))
        //    {
        //        int entityId = Convert.ToInt32(connection.ExecuteScalar(newSql));
        //        Assert.Greater(entityId, 1);
        //    }





        //}

    }
}
