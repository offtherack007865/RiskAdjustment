using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskAdjustment.Data
{
    internal class ConnectionString
    {
        internal string Value { get; set; }

        internal ConnectionString()
        {
#if DEBUG
            this.Value = ConfigurationManager.ConnectionStrings["development"].ToString();
#elif STAGING
            this.Value = ConfigurationManager.ConnectionStrings["staging"].ToString();
#else
            this.Value = ConfigurationManager.ConnectionStrings["release"].ToString();
#endif
        }

        internal ConnectionString(DbType dbType)
        {
            if (dbType == DbType.HCC)
            {
#if DEBUG
                this.Value = ConfigurationManager.ConnectionStrings["development"].ToString();
#elif STAGING
                this.Value = ConfigurationManager.ConnectionStrings["staging"].ToString();
#else
                this.Value = ConfigurationManager.ConnectionStrings["release"].ToString();
#endif
            }

            if (dbType == DbType.PVP)
            {
#if DEBUG
                this.Value = ConfigurationManager.ConnectionStrings["pvp-development"].ToString();
#elif STAGING
                this.Value = ConfigurationManager.ConnectionStrings["pvp-staging"].ToString(); 
#else
                this.Value = ConfigurationManager.ConnectionStrings["pvp-live"].ToString();

#endif
            }
        }

        internal ConnectionString(string connectionString)
        {
            this.Value = connectionString;
        }
    }
}
