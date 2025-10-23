using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using NUnit.Framework;
using RiskAdjustment.Data;
using RiskAdjustment.Logic.Hcc;
using Unity;

namespace RiskAdjustment.z.UnityTests.Integration.HCC_Worklist
{
    [TestFixture]
    public class MenuTests
    {
        private IHccLogic _logic;

        public MenuTests()
        {
            this._logic = UnityConfig.Container.Resolve<IHccLogic>();
        }

        [TestCase()]
        public void GetContractItems_ShouldReturnEnumerableList()
        {
            List<string> values = this._logic.GetContractMenuItems();
            Assert.Greater(values.Count, 0);
        }

        [TestCase()]
        public void GetContractCounts_ShouldBeGreaterThanZero()
        {
            Dictionary<string, int> values = new Dictionary<string, int>();
            //values = this._logic.GetContractCounts(DateTime.Now.ToString());
            Assert.Greater(values.Count, 0);

        }
    }
}
