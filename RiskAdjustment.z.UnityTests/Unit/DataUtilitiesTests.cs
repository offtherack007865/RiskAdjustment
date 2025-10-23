using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using RiskAdjustment.Data;

namespace RiskAdjustment.z.UnityTests.Unit
{
    [TestFixture]
    class DataUtilitiesTests
    {

        [TestCase]
        public void ConvertToSqlFormattedDateTime()
        {
            string sourceDate = "1/1/2020";
            string expectedDate = "2020-01-01 00:00:00.000";

            StringAssert.AreEqualIgnoringCase(expectedDate, DataUtilities.ConvertToSqlFormattedDateTime(sourceDate));


        }
    }
}
