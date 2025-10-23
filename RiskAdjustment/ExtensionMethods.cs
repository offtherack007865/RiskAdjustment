using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RiskAdjustment
{
    public class ExtensionMethods
    {
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
    }
}