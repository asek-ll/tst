using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TsT.Components;

namespace TsT.Modules.Jira
{
    public class JiraManager
    {
        private readonly JiraRestApiHelper _rest;

        public JiraManager(JiraRestApiHelper rest)
        {
            _rest = rest;
        }

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

            var client = _rest.GetClient();
            var httpResponseMessage =
                await client.PutAsync(string.Format("/rest/plugins/1.0/{0}-key", key), pluginContent);

            client.Dispose();
            return httpResponseMessage.StatusCode;
        }

        public async Task<HttpStatusCode> UpdatePluginEvabledState(string pluginKey, bool enabled)
        {
            var plugin = await GetPlugin(pluginKey);
            plugin["enabled"] = enabled;

            return await UpdatePlugin(pluginKey, plugin);
        }

        public async Task<JToken> InstallPluginToLocalServer(string key, string path)
        {
            var url = "file:///" + path.Replace("\\", "/");
            dynamic dataJsonObject = new JObject();

            dataJsonObject.pluginUri = url;

            var content = new StringContent(dataJsonObject.ToString(), null, "application/vnd.atl.plugins.install.uri+json");

            //content.Headers.Add("content-type", "application/vnd.atl.plugins.install.uri+json");

            return await InstallPlugin(content);
        }

        private async Task<JToken> InstallPlugin(HttpContent requestContent)
        {
            var upmToken = await _rest.GetUpmToken();

            using (var client = _rest.GetClient())
            {
                using (var response = await client.PostAsync("/rest/plugins/1.0/?token=" + upmToken, requestContent))
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var responseJsonObject = JObject.Parse(content);

                    var selfLink = responseJsonObject["links"]["self"];

                    return selfLink;
                }
            }
        }

        public async Task<JToken> InstallPlugin(string key, string path)
        {
            dynamic dataJsonObject = new JObject();

            dataJsonObject.pluginUri = path;

            var multipartContent = new MultipartFormDataContent();

            var pluginContent = new ByteArrayContent(File.ReadAllBytes(path));

            var fileName = Path.GetFileName(path);

            multipartContent.Add(pluginContent, "plugin", fileName);

            return await InstallPlugin(multipartContent);
        }

        public async Task<bool> CheckLink(string link, Func<bool, int, bool> callback)
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false
            };
            using (var client = _rest.GetClient(handler))
            {
                int i = 0;
                while (true)
                {
                    i += 1;
                    using (var response = await client.GetAsync(link))
                    {
                        if (response.StatusCode == HttpStatusCode.SeeOther)
                        {
                            callback(true, i);
                            return true;
                        }

                        if (!callback(false, i))
                        {
                            return false;
                        }
                    }

                    await Task.Delay(1000);
                }

            }
        }

        public async Task<bool> UninstallPlugin(string key)
        {
            using (var client = _rest.GetClient())
            {
                using (var httpResponseMessage = await client.DeleteAsync(string.Format("/rest/plugins/1.0/{0}-key", key)))
                {
                    return httpResponseMessage.StatusCode == HttpStatusCode.NoContent;
                }
            }
        }

        public async Task<bool> setWebSudo()
        {
            using (var client = _rest.GetClient())
            {
                using (var httpResponseMessage = await client.GetAsync("jira/secure/admin/WebSudoAuthenticate.jspa?webSudoPassword=" + _rest.Password))
                {
                    return httpResponseMessage.StatusCode == HttpStatusCode.Found;
                }
            }
        }
    }
}