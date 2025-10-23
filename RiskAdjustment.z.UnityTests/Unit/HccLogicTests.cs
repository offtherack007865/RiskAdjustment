using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using RiskAdjustment.Data.Models;
using RiskAdjustment.Logic.Hcc;
using Unity;

namespace RiskAdjustment.z.UnityTests.Unit
{
    [TestFixture]
    public class HccLogicTests
    {
        private IHccLogic _logic;

        public HccLogicTests()
        {
            this._logic = UnityConfig.Container.Resolve<IHccLogic>();
        }

        [TestCase()]
        public void SetPriorityOne()
        {
            WorklistEntry e = new WorklistEntry();
            e.ICD10ClaimDiagCode01 = "I634587";
            this._logic.SetPriority(e);
            Assert.AreEqual(1, e.Priority);
        }

        //[TestCase("9895599")]
        //public void IsRecordLocked_ShouldReturnTrue(string chargeid)
        //{
        //    WorklistEntry e = new WorklistEntry()
        //    {
        //        ChargeID = Convert.ToInt32(chargeid)
        //    };
        //    bool isRecordLocked = this._logic.IsRecordLocked(e);
        //    Assert.True(isRecordLocked);
        //}
    }
}
