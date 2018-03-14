using TsT.Components;

namespace TsT.Modules.ClientProxy
{
    public class ClientProxyPlugin : IModule
    {
        private readonly ProxyManager _proxyManager;
        private readonly Logger _logger;
        private readonly MainForm _mainForm;
        private readonly Config _config;

        public ClientProxyPlugin(Config config, Logger logger, MainForm mainForm)
        {
            _logger = logger;
            _proxyManager = new ProxyManager(config, _logger);
            _mainForm = mainForm;
            _config = config;
        }

        public void OnInit()
        {
            if (_config.GetProperty<string>("yaxy.location") == null)
            {
                return;
            }
            _mainForm.AddAction("Test in Proxy", () => _logger.Log("test", MessageType.OK));
            _mainForm.AddAction("ProxyOn", () =>
            {
                _proxyManager.SetProxyEnabled(true);
                _proxyManager.SetProxyGlobalEnabled(true);
            });
        }

        public void OnDestroy()
        {
        }
    }
}