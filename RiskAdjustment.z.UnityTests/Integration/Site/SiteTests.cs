using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using RiskAdjustment.Logic.Global;
using Unity;

namespace RiskAdjustment.z.UnityTests.Integration.Site
{
    [TestFixture]
    public class SiteTests
    {
        private ISiteLogic _siteLogic;

        public SiteTests()
        {
            this._siteLogic = UnityConfig.Container.Resolve<ISiteLogic>();
        }

        [TestCase]
        public void GetAllSites()
        {
            List<Data.Models.RefTables.Site> sites = this._siteLogic.GetAllSites();
            Assert.Greater(sites.Count, 1);
        }

        [TestCase("PCP131")]
        public void GetSiteBySiteNumber(string siteNumber)
        {
            Data.Models.RefTables.Site site = this._siteLogic.GetSite(siteNumber);
            Assert.NotNull(site);

        }
        [TestCase(1)]
        public void GetSiteById(string id)
        {
            Data.Models.RefTables.Site site = this._siteLogic.GetSite(id);
            Assert.NotNull(site);

        }
    }
}
