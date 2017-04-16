using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using TsT.Components;
using TsT.Entities;

namespace TsT.Plugins.Jira
{
    public class JiraPlugin : IPlugin
    {
        private readonly PluginManager _pluginManager;
        private readonly JiraManager _jiraManager;
        private readonly Logger _logger;
        private readonly MainForm _mainForm;

        public JiraPlugin(PluginManager pluginManager, Config config, Logger logger, MainForm mainForm)
        {
            _pluginManager = pluginManager;
            _jiraManager = new JiraManager(config);
            _logger = logger;
            _mainForm = mainForm;
        }

        public void OnInit()
        {
            _pluginManager.PluginsUpdated += FetchPluginJiraState;
            FetchPluginJiraState(_pluginManager.Plugins);

            _mainForm.AddAction("Enable", EnablePlugins);
            _mainForm.AddAction("Disable", DisablePlugins);
            _mainForm.AddAction("Redeploy", RedeployPlugin);
        }

        public void OnDestroy()
        {
        }

        private async void SetPluginsEnabled(bool enabled)
        {
            foreach (var plugin in _mainForm.GetSelectedPlugins())
            {
                var httpStatusCode = await _jiraManager.UpdatePluginEvabledState(plugin.Key, enabled);
                _logger.Log(string.Format("{0} enabled {1} is {2}", plugin.Key, enabled, httpStatusCode));
                if (httpStatusCode == HttpStatusCode.OK)
                {
                    plugin.Enabled = enabled;
                }
            }
        }

        private void EnablePlugins()
        {
            SetPluginsEnabled(true);
        }

        private void DisablePlugins()
        {
            SetPluginsEnabled(false);
        }

        private static string GetFileUrl(string path)
        {
            var targetPath = Path.Combine(path, "target");
            var jarRegex = new Regex(".+\\.jar");
            var testJarRegex = new Regex(".+\\-tests.jar");
            var firstJar = Directory.GetFiles(targetPath).First(p => jarRegex.IsMatch(p) && !testJarRegex.IsMatch(p));

            if (firstJar == null)
            {
                return null;
            }

            return "file:///" + firstJar.Replace("\\", "/");
        }

        private async void RedeployPlugin()
        {
            foreach (var plugin in _mainForm.GetSelectedPlugins())
            {
                var fileUrl = GetFileUrl(plugin.PomPath);
                if (fileUrl == null)
                {
                    continue;
                }

                var installCode = await _jiraManager.UninstallPlugin(plugin.Key);
                _logger.Log(string.Format("Uninstall plugin {0} is {1}", plugin.Key, installCode));

                var link = await _jiraManager.InstallPlugin(plugin.Key, fileUrl);
                _logger.Log(string.Format("Install plugin {0} is {1}", plugin.Key, link));
            }
        }

        private void FetchPluginJiraState(IEnumerable<PluginModule> plugins)
        {
            _jiraManager.GetEnabledPluginKeys()
                .ContinueWith(task =>
                    {
                        IEnumerable<string> enabledPluginKeys = task.Result;

                        foreach (var plugin in plugins)
                        {
                            if (!enabledPluginKeys.Contains(plugin.Key)) continue;

                            plugin.Enabled = true;
                            _logger.Log(plugin.Key + " enabled!!!");
                        }
                    }
                );
        }
    }
}