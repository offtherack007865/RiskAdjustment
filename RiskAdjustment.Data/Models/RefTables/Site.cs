using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskAdjustment.Data.Models.RefTables
{

    public class Site
    {
        public int Id { get; set; }
        public string SiteNumber { get; set; }
        public string Name { get; set; }
        public string County { get; set; }
        public bool Active { get; set; }
    }
}
