using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using TsT.Components;
using TsT.Entities;

namespace TsT
{
    public partial class MainForm : Form
    {
        private readonly PluginManager _pluginManager;
        private int _top;

        public MainForm(Logger logger, PluginManager pluginManager)
        {
            InitializeComponent();
            _pluginManager = pluginManager;

            _pluginManager.PluginsUpdated += RenderPlugins;

            RenderPlugins(_pluginManager.Plugins);

            logger.OnLogging += (s, e) =>
            {
                LogView.AppendText(e.Message + Environment.NewLine);
                LogView.ScrollToCaret();
            };
        }

        public void AddAction(string name, Action action)
        {
            var button = new Button
            {
                Text = name,
                Dock = DockStyle.Top,
                Location = new Point(0, _top)
            };
            button.Click += (x, y) => action.Invoke();
            _top += 24;
            MainSplit.Panel2.Controls.Add(button);
        }

        public IEnumerable<PluginModule> GetSelectedPlugins()
        {
            var plugins = _pluginManager.Plugins;

            var selectedPlugins = new List<PluginModule>();
            for (var i = 0; i < PluginsList.Items.Count; i++)
            {
                var pluginsListItem = PluginsList.Items[i];
                if (pluginsListItem.Selected)
                {
                    selectedPlugins.Add(plugins[i]);
                }
            }

            return selectedPlugins;
        }


        private void RenderPlugins(IEnumerable<PluginModule> plugins)
        {
            PluginsList.Items.Clear();

            foreach (var plugin in plugins)
            {
                var status = plugin.Enabled ? "ON" : "OFF";
                var item = new ListViewItem(new[] {plugin.Name, status, plugin.Branch}) {Name = "1"};

                PluginsList.Items.Add(item);
                plugin.PropertyChanged += PluginPropertyChanged;
            }
        }

        private void PluginPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var plugin = (PluginModule) sender;
            var index = _pluginManager.Plugins.IndexOf(plugin);

            if (e.PropertyName == "enabled")
            {
                var status = plugin.Enabled ? "ON" : "OFF";
                PluginsList.Items[index].SubItems[1].Text = status;
            }

            if (e.PropertyName == "branch")
            {
                PluginsList.Items[index].SubItems[2].Text = plugin.Branch;
            }
        }
    }
}