using System.Collections.Generic;
using System.Linq;
using TsT.Entities;

namespace TsT.Components
{
    public class PluginModuleManager
    {
        private List<PluginModule> _plugins = new List<PluginModule>();

        public List<PluginModule> Plugins
        {
            set
            {
                _plugins = value;
                OnPluginsUpdated();
            }
            get { return _plugins; }
        }

        public delegate void PluginsUpdatedContainer(IEnumerable<PluginModule> pluigns);

        public event PluginsUpdatedContainer PluginsUpdated;

        protected virtual void OnPluginsUpdated()
        {
            var handler = PluginsUpdated;
            if (handler != null) handler(_plugins);
        }

        public PluginModule getPluginByType<T>()
        {
            foreach (var plugin in _plugins)
            {
                if (plugin is T)
                {
                    return plugin;
                }
            }
            return null;
        }

        public IEnumerable<PluginModule> GetSelectedPlugins()
        {
            return _plugins.Where(plugin => plugin.Selected).ToList();
        }
    }
}