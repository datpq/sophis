using Eff.ToolkitReporting.NET.Interfaces;
using Eff.ToolkitReporting.NET.Services;
using Ninject;
using Ninject.Modules;

// ReSharper disable once CheckNamespace
namespace Eff.ToolkitReporting.NET
{
    public class CompositionRoot
    {
        private static IKernel _ninjectKernel;

        public static void Wire(INinjectModule module)
        {
            _ninjectKernel = new StandardKernel(module);
        }

        public static T Resolve<T>()
        {
            return _ninjectKernel.Get<T>();
        }
    }

    public class ApplicationModule : NinjectModule
    {
        public override void Load()
        {
            Bind(typeof(IReportService)).To(typeof(ReportService));
        }
    }
}
