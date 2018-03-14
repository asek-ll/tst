using Ninject.Modules;
using TsT.Modules.Mvn;

namespace TsT.Components
{
    public class Bindings : NinjectModule
    {
        public override void Load()
        {
            Bind<Logger>().To<Logger>().InSingletonScope();
            Bind<Config>().To<Config>().InSingletonScope();
            Bind<MainForm>().To<MainForm>().InSingletonScope();
            Bind<PluginModuleManager>().To<PluginModuleManager>().InSingletonScope();
            Bind<Utils>().To<Utils>().InSingletonScope();
            Bind<PomReader>().To<PomReader>().InSingletonScope();
        }
    }
}