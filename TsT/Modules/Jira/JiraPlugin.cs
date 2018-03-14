using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using TsT.Components;
using TsT.Entities;
using TsT.Modules.Mvn;

namespace TsT.Modules.Jira
{

    class RemoteServerEntry
    {
        [JsonProperty("url")]
        public string Title { get; set; }
        [JsonProperty("name")]
        public string Command { get; set; }
    }

    public class JiraPlugin : IModule
    {
        private readonly PluginModuleManager _pluginModuleManager;
        private readonly JiraManager _defaultJiraManager;
        private readonly Logger _logger;
        private readonly MainForm _mainForm;
        private readonly PomReader _pomReader;
        private readonly Config _config;
        private readonly JiraServerController _controller;

        public JiraPlugin(
            PluginModuleManager pluginModuleManager,
            Config config,
            Logger logger,
            MainForm mainForm,
            PomReader pomReader,
            Utils utils
            )
        {
            _pluginModuleManager = pluginModuleManager;
            _defaultJiraManager = new JiraManager(config);
            _logger = logger;
            _mainForm = mainForm;
            _pomReader = pomReader;
            _config = config;
            _controller = new JiraServerController(_config, _logger, utils);
        }

        public void OnInit()
        {
            _pluginModuleManager.PluginsUpdated += (plugin) => FetchPluginModuleJiraState(plugin);

            _mainForm.AddAction("Do", BuildAndRestart);

            _mainForm.AddAction("Enable", EnablePlugins);
            _mainForm.AddAction("Disable", DisablePlugins);
            _mainForm.AddAction("Redeploy", BuildAndRedeploy);

            _mainForm.AddAction("Startup", async () =>
            {
                await _controller.Startup();
            });

            _mainForm.AddAction("Shutdown", async () =>
            {
                await _controller.Shutdown();
            });
            _mainForm.AddAction("Kill", async () =>
            {
                await _controller.Kill();
            });

            _mainForm.AddAction("Restart", async () =>
            {
                await _controller.Kill();
                await _controller.Startup();
            });

            _mainForm.AddAction("Logs", async () =>
            {
                await _controller.OpenLogsDir();
            });

            _mainForm.AddAction("Copy", () =>
            {
                CopyPluginsToJira(_pluginModuleManager.GetSelectedPlugins());
            });

            _mainForm.AddAction("Update", () =>
            {
                FetchPluginModuleJiraState(_pluginModuleManager.Plugins);
            });

            var servers = _config.GetProperties<JObject>("servers");

            if (servers != null)
            {

                var deployRemote = new ToolStripMenuItem
                {
                    Text = "Deploy to remote"
                };
                _mainForm.contextMenu.Items.Add(deployRemote);

                var serversMenu = new ToolStripMenuItem
                {
                    Text = "Copy and restart",
                    Name = "remoteServersMenu"
                };

                _mainForm.menuStrip1.Items.Add(serversMenu);

                foreach (var entry in servers)
                {
                    var remoteServer = entry.ToObject<RemoteServerEntry>();

                    var remoteServerReloadItem = new ToolStripMenuItem
                    {
                        Text = remoteServer.Title
                    };
                    remoteServerReloadItem.Click += (s, e) =>  BuildAndRestartRemoteServer(remoteServer);

                    serversMenu.DropDownItems.Add(remoteServerReloadItem);

                    var server = remoteServer.Title;

                    _logger.Log("Register remote server: " + server);

                    var openItem = new ToolStripMenuItem
                    {
                        Text = server
                    };

                    openItem.Click += (o, e) =>
                    {
                        var manager = new JiraManager(new JiraRestApiHelper(
                            server,
                            _config.GetProperty<string>("jira.user.key"),
                            _config.GetProperty<string>("jira.user.password")
                            ));

                        BuildAndRedeploy(manager);
                    };

                    deployRemote.DropDownItems.Add(openItem);

                }
            }


            FetchPluginModuleJiraState(_pluginModuleManager.Plugins);
        }

        public void OnDestroy()
        {
        }

        private async void SetPluginsEnabled(JiraManager jiraManager, bool enabled)
        {
            foreach (var plugin in _pluginModuleManager.GetSelectedPlugins())
            {
                var httpStatusCode = await jiraManager.UpdatePluginEvabledState(plugin.Key, enabled);

                var isDone = httpStatusCode == HttpStatusCode.OK;

                _logger.Log(string.Format("{0} enabled {1} is {2}", plugin.Key, enabled, httpStatusCode), isDone ? MessageType.OK : MessageType.ERROR);

                if (isDone)
                {
                    plugin.Enabled = enabled;
                }
            }
        }

        private void EnablePlugins()
        {
            SetPluginsEnabled(_defaultJiraManager, true);
        }

        private void DisablePlugins()
        {
            SetPluginsEnabled(_defaultJiraManager, false);
        }

        private static string GetFileUrl(string path)
        {
            var targetPath = Path.Combine(path, "target");
            var jarRegex = new Regex(".+\\.jar");
            var testJarRegex = new Regex(".+\\-tests.jar");
            var firstJar = Directory.GetFiles(targetPath).First(p => jarRegex.IsMatch(p) && !testJarRegex.IsMatch(p));

            return firstJar;
        }

        private async void BuildAndRedeploy(JiraManager jiraManager, bool isLocalMode)
        {
            var plugins = _pluginModuleManager.GetSelectedPlugins();
            var isSuccessBuild = await _pomReader.Build(plugins, true);
            if (isSuccessBuild)
            {
                try
                {
                    RedeployPlugin(jiraManager, plugins, isLocalMode);
                }
                catch (Exception)
                {
                    _logger.Error("Error on plugin deployment");
                }
            }
        }

        private void BuildAndRedeploy(JiraManager jiraManager)
        {
            BuildAndRedeploy(jiraManager, false);
        }

        private void BuildAndRedeploy()
        {
            BuildAndRedeploy(_defaultJiraManager, true);
        }

        private async void RedeployPlugin(JiraManager jiraManager, IEnumerable<PluginModule> plugins, bool localServerMode)
        {
            await jiraManager.setWebSudo();

            var enabledKeysBefore = await jiraManager.GetEnabledPluginKeys();

            var allPluginsIntalled = true;

            foreach (var plugin in plugins)
            {
                var fileUrl = GetFileUrl(plugin.PomPath);
                if (fileUrl == null)
                {
                    continue;
                }

                _logger.Log(string.Format("Start uninstall plugin {0}...", plugin.Key));
                var uninstallState = await jiraManager.UninstallPlugin(plugin.Key);
                _logger.Log(string.Format("Uninstall plugin {0} is {1}", plugin.Key, uninstallState), uninstallState ? MessageType.OK : MessageType.ERROR);

                _logger.Log(string.Format("Start install plugin {0}...", plugin.Key));
                string link;

                if (localServerMode)
                {
                    link = (await jiraManager.InstallPluginToLocalServer(plugin.Key, fileUrl)).ToString();
                }
                else
                {
                    link = (await jiraManager.InstallPlugin(plugin.Key, fileUrl)).ToString();
                }

                _logger.Log(string.Format("Install plugin {0} is {1}", plugin.Key, link));

                var plugin1 = plugin;
                var isPluginInstalled = await jiraManager.CheckLink(link.ToString(), (isSuccess, attemptCount) =>
                {
                    if (isSuccess)
                    {
                        _logger.Success(string.Format("Install plugin {0} successfull", plugin1.Key));
                        return true;
                    }

                    if (attemptCount > 10)
                    {
                        _logger.Error(string.Format("Install plugin {0} failed", plugin1.Key));
                        return false;
                    }

                    _logger.Log(string.Format("Install plugin {0} attempt {1}", plugin1.Key, attemptCount));
                    return true;
                });

                if (!isPluginInstalled)
                {
                    allPluginsIntalled = false;
                }
            }

            if (!allPluginsIntalled)
            {
                return;
            }

            var enabledKeysAfter = await jiraManager.GetEnabledPluginKeys();

            var notEnabledKeys = enabledKeysBefore.Except(enabledKeysAfter);

            if (notEnabledKeys.Count() > 0)
            {
                _logger.Log("Disabled keys: " + String.Join(", ", notEnabledKeys) + ", attempt to re-enable");


                foreach (var key in notEnabledKeys)
                {
                    var httpStatusCode = await jiraManager.UpdatePluginEvabledState(key, true);

                    var isDone = httpStatusCode == HttpStatusCode.OK;

                    _logger.Log(string.Format("{0} enabled is {1}", key, httpStatusCode), isDone ? MessageType.OK : MessageType.ERROR);
                }

            }

        }

        private async void FetchPluginModuleJiraState(IEnumerable<PluginModule> plugins)
        {
            List<String> enabledPluginKeys;
            try
            {
                enabledPluginKeys = await _defaultJiraManager.GetEnabledPluginKeys();
            }
            catch (Exception)
            {
                _logger.Error("Error on fetching plugins state");
                return;
            }

            foreach (var plugin in plugins)
            {
                if (!enabledPluginKeys.Contains(plugin.Key)) continue;

                plugin.Enabled = true;
            }
        }

        private void CopyPluginsToJira(IEnumerable<PluginModule> plugins)
        {
            foreach (var plugin in plugins)
            {
                var fileUrl = GetFileUrl(plugin.PomPath);
                _controller.CopyPlugin(fileUrl);
            }
        }

        private async void BuildAndRestart()
        {
            var plugins = _pluginModuleManager.GetSelectedPlugins();
            var isSuccessBuild = await _pomReader.Build(plugins);
            if (isSuccessBuild)
            {
                await _controller.Kill();
                CopyPluginsToJira(plugins);
                await _controller.Startup();
            }
        }

        private async void BuildAndRestartRemoteServer(RemoteServerEntry server)
        {
            var plugins = _pluginModuleManager.GetSelectedPlugins();
            var isSuccessBuild = await _pomReader.Build(plugins);
            if (isSuccessBuild)
            {
                //await _controller.Kill();
                //CopyPluginsToJira(plugins);
                //await _controller.Startup();
            }
        }
    }
}