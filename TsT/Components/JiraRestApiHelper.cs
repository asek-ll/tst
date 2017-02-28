using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace TsT.Components
{
    public class JiraRestApiHelper
    {
        private readonly Config _config;

        public JiraRestApiHelper(Config config)
        {
            _config = config;
        }

        public async Task<JObject> DoJsonRequest(string method, string url)
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

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization",
                    "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(jiraUser + ":" + jiraUserPassword))
                );
                var resp = await client.GetAsync(jiraUrl + "/rest" + url);
                var content = await resp.Content.ReadAsStringAsync();
                return JObject.Parse(content);
            }

            var request = WebRequest.Create(jiraUrl + "/rest" + url);
            request.Method = method;
            request.ContentType = "application/json";

            request.Headers["Authorization"] =
                "Basic " +
                Convert.ToBase64String(
                    Encoding.Default.GetBytes(jiraUser + ":" + jiraUserPassword)
                );


            var response = request.GetResponse();

            var responseStream = response.GetResponseStream();

            if (responseStream == null) return null;

            var myStreamReader = new StreamReader(responseStream, Encoding.Default);

            var pageContent = myStreamReader.ReadToEnd();

            return JObject.Parse(pageContent);
        }
    }
}