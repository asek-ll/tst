using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TsT.Components;

namespace TsT.Modules.Jira
{
    public class JiraRestApiHelper
    {
        private string _url;
        private string _username;
        private string _password;

        public string Password
        {
            get
            {
                return _password;
            }
        }

        public JiraRestApiHelper(Config config)
        {
            _url = config.GetProperty<string>("jira.url");

            if (_url == null)
            {
                throw new ArgumentException("Specify jira.url property in config file");
            }

            _username = config.GetProperty<string>("jira.user.key");

            if (_username == null)
            {
                throw new ArgumentException("Specify jira.user.key property in config file");
            }

            _password = config.GetProperty<string>("jira.user.password");

            if (_password == null)
            {
                throw new ArgumentException("Specify jira.user.password property in config file");
            }
        }

        public JiraRestApiHelper(String url, String username, String password)
        {
            _url = url;
            _username = username;
            _password = password;
        }

        public delegate Task<HttpResponseMessage> JsonRequestCallback(HttpClient client);

        public async Task<JObject> DoGetJsonRequest(string url)
        {
            using (var client = GetClient())
            {
                using (var resp = await client.GetAsync(url))
                {
                    var content = await resp.Content.ReadAsStringAsync();
                    return JObject.Parse(content);
                }
            }
        }

        public async Task<string> GetUpmToken()
        {
            using (var client = GetClient())
            {
                using (var httpResponseMessage = await client.GetAsync("/rest/plugins/1.0/"))
                {
                    IEnumerable<string> values;
                    return httpResponseMessage.Headers.TryGetValues("upm-token", out values) ? values.First() : null;
                }
            }
        }

        public HttpClient GetClient()
        {
            return GetClient(new HttpClientHandler());
        }

        public HttpClient GetClient(HttpMessageHandler handler)
        {
            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("Authorization",
                "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(_username + ":" + _password))
            );

            client.BaseAddress = new Uri(_url);
            return client;
        }
    }
}