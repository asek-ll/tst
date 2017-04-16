using TsT.Components;

namespace TsT.Plugins.ClientProxy
{
    public class ClientProxyPlugin : IPlugin
    {
        private readonly ProxyManager _proxyManager;
        private readonly Logger _logger;
        private readonly MainForm _mainForm;

        public ClientProxyPlugin(Config config, Logger logger, MainForm mainForm)
        {
            _logger = logger;
            _proxyManager = new ProxyManager(config, _logger);
            _mainForm = mainForm;
        }

        public void OnInit()
        {
            _mainForm.AddAction("Test in Proxy", () => _logger.Log("test"));
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