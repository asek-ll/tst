using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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

        public JiraRemoteServerController(Logger logger, Utils utils, String jiraHome, String serverName)
        {
            _home = jiraHome;
            _logger = logger;
            _utils = utils;
            _serverName = serverName;
        }

        public string PluginDir
        {
            get
            {
                return Path.Combine(_home, "plugins", "installed-plugins");
            }
        }

        private int getTomCatPid()
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "tasklist.exe",
                    Arguments = "/FO CSV /FI \"IMAGENAME eq tomcat8.exe.x64\" /S \\\\ " + _serverName,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                }
            };

            proc.Start();

            proc.WaitForExit();

            string output = proc.StandardOutput.ReadToEnd();

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
            var pid = getTomCatPid();

            if (pid < 0)
            {
                return;
            }

            _logger.Log("killing jira by pid " + pid);

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "taskkill.exe",
                    Arguments = "/pid " + pid + " /T /S \\" + _serverName,
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
    }
}
