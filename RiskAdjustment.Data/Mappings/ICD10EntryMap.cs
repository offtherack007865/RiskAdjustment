using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper.FluentMap.Mapping;
using RiskAdjustment.Data.Models.RefTables;

namespace RiskAdjustment.Data.Mappings
{
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    public class ICD10EntryMap : EntityMap<ICD10Entry>
    {
        public ICD10EntryMap()
        {
            Map(p => p.ESRD_Model_V21).ToColumn("ESRD_Model-V21");
            Map(p => p.ESRD_Model_V22).ToColumn("CMS_HCC_Model-V22");
            Map(p => p.ESRD_Model_V24).ToColumn("CMS_HCC_Model-V24");
            Map(p => p.ESRD_Model_V28).ToColumn("CMS_HCC_Model-V28");
        }
        
    }
}
