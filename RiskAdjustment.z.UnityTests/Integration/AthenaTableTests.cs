using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc.Html;
using NUnit.Framework;
using NUnit.Framework.Internal;
using RiskAdjustment.Data;
using RiskAdjustment.Data.Models;

namespace RiskAdjustment.z.UnityTests.Integration
{
    [TestFixture]
    public class AthenaTableTests
    {
        private DbQuery _query;

        public AthenaTableTests()
        {
            this._query = new DbQuery();
        }


        [TestCase("10000203")]
        public void CanAccessDB(string chargeid)
        {
            string sql = $"SELECT * FROM HCC_Athena_Worklist WHERE chargeid = '{chargeid}'";
            WorklistEntry entry = this._query.ExecuteSingle<WorklistEntry>(sql);
            Assert.NotNull(entry);
        }

    }
}
