using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using RiskAdjustment.Data.Models;
using RiskAdjustment.Logic.Hcc;
using Unity;

namespace RiskAdjustment.z.UnityTests.Integration.HCC_Worklist
{
    [TestFixture]
    public class WorklistQueries
    {
        private IHccLogic _logic;

        public WorklistQueries()
        {
            this._logic = UnityConfig.Container.Resolve<IHccLogic>();
        }

        [TestCase("4/1/2021", "HumanaMA")]
        public void GetWorklistItems_Async_ShouldReturnGreaterThanZero(string fileDate, string contract)
        {
            Task<List<WorklistEntry>> entries = this._logic.GetUnlockedWorklistEntriesForDateAsync(fileDate, contract);
            Assert.Greater(entries.Result.Count, 0);
        }

        [TestCase("10000203")]
        public void GetWorklistEntry_Async_ShouldReturnValid(string chargeid)
        {
            Task<WorklistEntry> entry = this._logic.GetWorklistEntryAsync(chargeid);
            Assert.NotNull(entry);
        }

    }
}
