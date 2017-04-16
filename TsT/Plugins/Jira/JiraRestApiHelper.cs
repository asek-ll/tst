using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using Newtonsoft.Json.Linq;
using TsT.Components;

namespace TsT.Plugins.Jira
{
    public class JiraRestApiHelper
    {
        private readonly Config _config;

        public JiraRestApiHelper(Config config)
        {
            _config = config;
        }

        public delegate Task<HttpResponseMessage> JsonRequestCallback(HttpClient client);


        public async Task<JObject> DoGetJsonRequest(string url)
        {
            return await DoJsonRequest(x => x.GetAsync(url));
        }

        public async Task<JObject> DoJsonRequest(JsonRequestCallback callback)
        {
            using (var client = GetClient())
            {
                var resp = await callback(client);
                var content = await resp.Content.ReadAsStringAsync();
                return JObject.Parse(content);
            }
        }

        public async Task<string> GetUpmToken(HttpClient client)
        {
            var httpResponseMessage = await client.GetAsync("/rest/plugins/1.0/");
            IEnumerable<string> values;
            return httpResponseMessage.Headers.TryGetValues("upm-token", out values) ? values.First() : null;
        }

        public HttpClient GetClient()
        {
            var jiraUrl = _config.GetProperty<string>("jira.url");

            if (jiraUrl == null)
            {
                throw new ArgumentException("Specify jira.url property in config file");
            }

            var jiraUser = _config.GetProperty<string>("jira.user.key");

            if (jiraUser == null)
            {
                throw new ArgumentException("Specify jira.user.key property in config file");
            }

            var jiraUserPassword = _config.GetProperty<string>("jira.user.password");

            if (jiraUserPassword == null)
            {
                throw new ArgumentException("Specify jira.user.password property in config file");
            }

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization",
                "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(jiraUser + ":" + jiraUserPassword))
            );

            client.BaseAddress = new Uri(jiraUrl);
            return client;
        }
    }
}