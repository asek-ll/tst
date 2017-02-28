using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TsT.Components
{
    public class JiraManager
    {
        private readonly Config _config;

        private readonly JiraRestApiHelper _rest;

        public JiraManager(Config config, JiraRestApiHelper jiraRest)
        {
            _config = config;
            _rest = jiraRest;
        }

        public async Task<List<string>> GetEnabledPluginKeys()
        {
            var response = await _rest.DoJsonRequest("GET", "/plugins/1.0/");

            return (
                from plugin in response["plugins"]
                let vendor = plugin["vendor"]
                where vendor != null && vendor.Value<string>("name") == "Transas Technologies"
                where plugin.Value<bool>("enabled")
                select plugin.Value<string>("key")
            ).ToList();
        }

        public void UpdatePluginEvabledState(string pluginKey, bool enabled)
        {
        }
    }
}