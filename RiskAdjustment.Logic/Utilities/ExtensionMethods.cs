using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using RiskAdjustment.Data.Models.RefTables;

namespace RiskAdjustment.Logic.Utilities
{
    public static class ExtensionMethods
    {
        public static string Capitalize(this string value)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower());
        }

        public static string GetRuntimeEnvironment()
        {
            #if DEBUG
                return "Debug";
            #elif STAGING
                return "Staging";
            #else
                return "Live";
            #endif

        }

        public static bool IsManagerOrAdmin(this User user)
        {
            if (user.Role == "Manager" || user.Role == "Admin")
                return true;
            return false;
        }
    }
}
