using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TsT.Components;

namespace TsT.Modules.Utility
{
    class CommandEntry
    {
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("command")]
        public string Command { get; set; }
        [JsonProperty("arguments")]
        public string Arguments { get; set; }
    }


    class Utility : IModule
    {
        private MainForm _form;
        private Logger _logger;
        private PluginModuleManager _plugins;
        private Config _config;
        private Utils _utils;

        public Utility(MainForm form,
            Logger logger,
            PluginModuleManager pluginModuleManager,
            Config config,
            Utils utils)
        {
            _form = form;
            _logger = logger;
            _plugins = pluginModuleManager;
            _config = config;
            _utils = utils;
        }

        public void OnDestroy()
        {
        }

        public void OnInit()
        {
            var openItem = new ToolStripMenuItem
            {
                Text = "Open"
            };
            openItem.Click += async (o, e) => await OpenPluginDir();

            _form.contextMenu.Items.Add(openItem);


            _form.PluginsList.MouseDoubleClick += async (o, e) => await OpenPluginDir();


            var entries = _config.GetProperties<JObject>("commands");

            foreach (var entry in entries)
            {
                var command = entry.ToObject<CommandEntry>();

                var commandItem = new ToolStripMenuItem
                {
                    Text = command.Title
                };

                commandItem.Click += async (o, e) => await ExecCmd(command.Command, command.Arguments);
                _form.contextMenu.Items.Add(commandItem);
            }
        }

        private async Task ExecCmdFromProperty(string propertyKey)
        {
            var fileManager = _config.GetProperty<string>(propertyKey);

            if (fileManager == null)
            {
                _logger.Error("Error: specify " + propertyKey + " property in config.json");
                return;
            }

            var arguments = _config.GetProperty<string>(propertyKey + ".arguments");

            await ExecCmd(fileManager, arguments);
        }

        private async Task OpenPluginDir()
        {
            var path = GetSelectedPluginPath();
            if (path != null)
            {
                await _utils.OpenDir(path);
            }
        }

        private string GetSelectedPluginPath()
        {
            var selectedPlugins = _plugins.GetSelectedPlugins();
            if (selectedPlugins.Any())
            {
                var plugin = selectedPlugins.First();
                return plugin.PomPath;
            }

            return null;
        }

        private async Task ExecCmd(string program, string arguments)
        {
            var path = GetSelectedPluginPath();

            if (path != null)
            {
                await _utils.ExecuteWithParam(path, program, arguments);
            }
        }

    }
}
