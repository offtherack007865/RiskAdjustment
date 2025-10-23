using System.Collections.Generic;
using RiskAdjustment.Data;
using RiskAdjustment.Data.Models.RefTables;
using System.Configuration;
using System.Threading.Tasks;
using RiskAdjustment.Data.Models.HccWorkflow;
using System.Linq;

namespace RiskAdjustment.Logic.Global
{

    public interface IICD10Logic
    {
        List<ICD10Entry> GetEntries();
    }
    public class ICD10Logic : IICD10Logic
    {
        public List<ICD10Entry> GetEntries()
        {
            string sql = "SELECT * FROM REF_CMS_RiskModel_ICD10";
            DbQuery query = new DbQuery();
            return  query.Execute<ICD10Entry>(sql);
        }

        public void PopulateGlobalICD10List()
        {
            ICD10Global.Entries = GetEntries(); 
            ICD10Global.Entries = ICD10Global.Entries.Where(x => x.ESRD_Model_V28 > 0).ToList();
        }
    }
}
