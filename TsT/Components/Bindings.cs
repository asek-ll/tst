using Ninject.Modules;

namespace TsT.Components
{
    public class Bindings : NinjectModule
    {
        public override void Load()
        {
            Bind<Logger>().To<Logger>().InSingletonScope();
            Bind<Config>().To<Config>().InSingletonScope();
            Bind<MainForm>().To<MainForm>().InSingletonScope();
            Bind<PluginManager>().To<PluginManager>().InSingletonScope();
        }
    }
}