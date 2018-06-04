using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TsT.Components;

namespace TsT.Modules.Jira
{
    class JiraRemoteServerController
    {
        private string _home;
        private Logger _logger;
        private Utils _utils;
        private string _serverName;

        public JiraRemoteServerController(Logger logger, Utils utils, RemoteServerEntry remoteServer)
        {
            _logger = logger;
            _utils = utils;
            _serverName = remoteServer.Name;
            _home = remoteServer.Home ?? "\\\\" + remoteServer.Name + "\\c$\\Program Files\\Atlassian\\Application Data\\JIRA\\";
        }

        public string PluginDir
        {
            get
            {
                return Path.Combine(_home, "plugins", "installed-plugins");
            }
        }

        private async Task<int> GetTomCatPid()
        {
            var result = await Utils.RunProcessWithOutput(new ProcessStartInfo
            {
                FileName = "tasklist.exe",
                Arguments =  "/S \\\\" + _serverName + " /FO CSV /FI \"IMAGENAME eq tomcat8.exe.x64\""
            });

            string output = result.Item2;

            var strings = output.Split('\n');

            if (strings.Length > 1)
            {
                var values = strings[1].Split(',');
                if (values.Length > 1)
                {
                    var pidInQutas = values[1];
                    return int.Parse(pidInQutas.Substring(1, pidInQutas.Length - 2));
                }
            }

            return -1;
        }

        public async Task Kill()
        {
            var pid = await GetTomCatPid();

            if (pid < 0)
            {
                _logger.Log("Can't find pid for jira");
                return;
            }

            _logger.Log("killing jira by pid " + pid);

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "taskkill.exe",
                    Arguments = "/pid " + pid + " /T /S \\\\" + _serverName,
                }
            };

            var code = await Utils.RunProcessAsync(proc);
            _logger.Log("Killing jira done with code: " + code);
        }

        public async Task OpenLogsDir()
        {
            await _utils.OpenDir(Path.Combine(_home, "log"));
        }

        public void CopyPlugin(string path)
        {
            var fileName = Path.GetFileName(path);
            File.Copy(path, Path.Combine(_home, "plugins", "installed-plugins", fileName), true);
        }

        public async Task<string> GetServiceName()
        {
            var res = await Utils.RunProcessWithOutput(new ProcessStartInfo
            {
                FileName = "sc.exe",
                Arguments = "\\\\" + _serverName + " query",
            });

            string output = res.Item2;

            var pattern = @"\SERVICE_NAME: (JIRA\S.+)";
            var matches = Regex.Matches(output, pattern);
            if (matches.Count > 0)
            {
                var match = matches[0];
                return match.Groups[1].Value;
            }

            return null;
        }

        private async Task StartServiceAsync(string serviceName)
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "sc",
                    Arguments = "\\\\" + _serverName + " start " + serviceName,
                }
            };

            var code = await Utils.RunProcessAsync(proc);
            _logger.Log("Starting jira service " + serviceName + " done with code: " + code);
        }

        public async Task Startup()
        {
            var serviceName = await GetServiceName();
            if ( serviceName == null )
            {
                _logger.Log("Can't find service for Jira");
            }
            else
            {
                await StartServiceAsync(serviceName);
            }
        }
    }
}
