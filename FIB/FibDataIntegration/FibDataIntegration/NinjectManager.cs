using FibDataIntegration.Services;
using Ninject;
using Ninject.Modules;

namespace FibDataIntegration
{
    public static class NinjectManager
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
            Bind(typeof(IDataStore)).To(typeof(DataStore));
        }
    }
}
