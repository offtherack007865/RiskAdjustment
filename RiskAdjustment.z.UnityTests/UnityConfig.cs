using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework.Internal;
using RiskAdjustment.Logic;
using RiskAdjustment.Logic.Global;
using RiskAdjustment.Logic.Hcc;
using Unity;

namespace RiskAdjustment.z.UnityTests
{
    public static class UnityConfig
    {

        private static Lazy<IUnityContainer> container =
            new Lazy<IUnityContainer>(() =>
            {
                var container = new UnityContainer();
                RegisterTypes(container);
                return container;
            });

        public static IUnityContainer Container = container.Value;

        public static void RegisterTypes(IUnityContainer container)
        {
            // NOTE: To load from web.config uncomment the line below.
            // Make sure to add a Unity.Configuration to the using statements.
            // container.LoadConfiguration();

            // TODO: Register your type's mappings here.
            // container.RegisterType<IProductRepository, ProductRepository>();
            container.RegisterType<IGlobalUserLogic, GlobalUserLogic>();
            container.RegisterType<ISiteLogic, SiteLogic>();
            container.RegisterType<IHccLogic, HccLogic>();
            container.RegisterType<IICD10Logic, ICD10Logic>();


        }
    }
}


