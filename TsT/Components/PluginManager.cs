using System.Collections.Generic;
using TsT.Entities;

namespace TsT.Components
{
    public class PluginManager
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
    }
}