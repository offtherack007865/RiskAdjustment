using RiskAdjustment.Data.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskAdjustment.Data.Models.RefTables
{
    public class ICD10Entry
    {
        public float ID { get; set; }
        public string DiagnosisCode { get; set; }
        public string Description { get; set; }

        //SQL Server floats are actually doubles in C#.  https://stackoverflow.com/questions/122523/why-is-a-sql-float-different-from-a-c-sharp-float
        [DbColumn("ESRD_Model-V21")]
        public double ESRD_Model_V21 { get; set; }

        [DbColumn("CMS_HCC_Model-V22")]
        public double ESRD_Model_V22 { get; set; }

        [DbColumn("CMS_HCC_Model-V24")]
        public double ESRD_Model_V24 { get; set; }

        [DbColumn("CMS_HCC_Model-V28")]
        public double ESRD_Model_V28 { get; set; }

    }
}
