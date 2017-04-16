using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TsT.Components;

namespace TsT.Plugins.Jira
{
    public class JiraManager
    {
        private readonly JiraRestApiHelper _rest;

        public JiraManager(Config config)
        {
            _rest = new JiraRestApiHelper(config);
        }

        public async Task<List<string>> GetEnabledPluginKeys()
        {
            var response = await _rest.DoGetJsonRequest("/rest/plugins/1.0/");

            return (
                from plugin in response["plugins"]
                let vendor = plugin["vendor"]
                where vendor != null && vendor.Value<string>("name") == "Transas Technologies"
                where plugin.Value<bool>("enabled")
                select plugin.Value<string>("key")
            ).ToList();
        }

        public async Task<JObject> GetPlugin(string key)
        {
            return await _rest.DoGetJsonRequest(string.Format("/rest/plugins/1.0/{0}-key", key));
        }

        private async Task<HttpStatusCode> UpdatePlugin(string key, JObject pluginJson)
        {
            var pluginContent = new StringContent(pluginJson.ToString(), null,
                "application/vnd.atl.plugins.plugin+json");

            using (var client = _rest.GetClient())
            {
                var httpResponseMessage =
                    await client.PutAsync(string.Format("/rest/plugins/1.0/{0}-key", key), pluginContent);

                return httpResponseMessage.StatusCode;
            }
        }

        public async Task<HttpStatusCode> UpdatePluginEvabledState(string pluginKey, bool enabled)
        {
            var plugin = await GetPlugin(pluginKey);
            plugin["enabled"] = enabled;

            return await UpdatePlugin(pluginKey, plugin);
        }

        public async Task<JToken> InstallPlugin(string key, string path)
        {
            dynamic dataJsonObject = new JObject();

            dataJsonObject.pluginUri = path;

            var postContent = new StringContent(dataJsonObject.ToString(), null,
                "application/vnd.atl.plugins.install.uri+json");

            using (var client = _rest.GetClient())
            {
                var upmToken = await _rest.GetUpmToken(client);
                var response = await client.PostAsync("/rest/plugins/1.0/?token=" + upmToken, postContent);
                var content = await response.Content.ReadAsStringAsync();
                var responseJsonObject = JObject.Parse(content);

                var selfLink = responseJsonObject["links"]["self"];

                return selfLink;
            }
        }

        public async Task<HttpStatusCode> UninstallPlugin(string key)
        {
            using (var client = _rest.GetClient())
            {
                var httpResponseMessage = await client.DeleteAsync(string.Format("/rest/plugins/1.0/{0}-key", key));
                return httpResponseMessage.StatusCode;
            }
        }
    }
}