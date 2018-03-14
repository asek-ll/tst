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
        private readonly PluginModuleManager _pluginModuleManager;
        private int _top;
        private Logger _logger;

        public MainForm(Logger logger, PluginModuleManager pluginModuleManager)
        {
            InitializeComponent();
            _pluginModuleManager = pluginModuleManager;

            _pluginModuleManager.PluginsUpdated += RenderPluginsModule;

            RenderPluginsModule(_pluginModuleManager.Plugins);

            logger.OnLogging += OnLogging;

            _logger = logger;
        }

        public void AddAction(string name, Action action)
        {
            var parent = MainSplit.Panel2;
            var button = new Button
            {
                Text = name,
                Dock = DockStyle.None,
                Location = new Point(0, _top),
                Font = new Font("Consolas", 14),
                Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top,
                Width = parent.Width,
                Height = 30,
            };
            button.Click += (x, y) =>
            {
                try
                {
                action.Invoke();
                } catch(Exception)
                {
                    _logger.Error("Error on executing action");
                }
            };
            _top += 32;
            parent.Controls.Add(button);
        }

        public delegate void RenderPluginsModules(IEnumerable<PluginModule> plugins);

        private void RenderPluginsModule(IEnumerable<PluginModule> plugins)
        {
            if (InvokeRequired)
            {
                Invoke(new RenderPluginsModules(RenderPluginsModule));
                return;
            }

            PluginsList.Items.Clear();

            foreach (var plugin in plugins)
            {
                var status = plugin.Enabled ? "ON" : "OFF";
                var item = new ListViewItem(new[] { plugin.Name, status, plugin.Branch }) { Name = "1" };

                PluginsList.Items.Add(item);
                plugin.PropertyChanged += PluginPropertyChanged;
            }
        }

        private void UpdatePlugin(PluginModule plugin, String propertyName)
        {
            var index = _pluginModuleManager.Plugins.IndexOf(plugin);

            if (propertyName == "enabled")
            {
                var status = plugin.Enabled ? "ON" : "OFF";
                PluginsList.Items[index].SubItems[1].Text = status;
            }

            if (propertyName == "branch")
            {
                PluginsList.Items[index].SubItems[2].Text = plugin.Branch;
            }
        }

        private void PluginPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var plugin = (PluginModule)sender;

            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdatePlugin(plugin, e.PropertyName)));
            }
            else
            {
                UpdatePlugin(plugin, e.PropertyName);
            }
        }


        private void SafeOnLogging(object sender, LogEventArgs e)
        {
            LogView.SelectionColor = Color.Black;
            LogView.SelectionBackColor = Color.White;
            LogView.AppendText(DateTime.Now.ToString("HH:mm:ss") + ": ");

            if (e.Type == MessageType.ERROR)
            {
                LogView.SelectionColor = Color.DarkRed;
                LogView.SelectionBackColor = Color.Yellow;
            }
            else if (e.Type == MessageType.OK)
            {
                LogView.SelectionColor = Color.Green;
            }
            LogView.AppendText(e.Message + Environment.NewLine);
            LogView.ScrollToCaret();
        }

        private void OnLogging(object sender, LogEventArgs args)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => SafeOnLogging(sender, args)));
            }
            else
            {
                SafeOnLogging(sender, args);
            }
        }

        private void PluginsList_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var contextMenuItem = PluginsList.GetItemAt(e.X, e.Y);

                for (var i = 0; i < PluginsList.Items.Count; i++)
                {
                    var pluginsListItem = PluginsList.Items[i];
                    pluginsListItem.Selected = pluginsListItem == contextMenuItem;
                }

                contextMenu.Show(Cursor.Position.X, Cursor.Position.Y);
            }
        }

        private void PluginsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var plugins = _pluginModuleManager.Plugins;

            for (var i = 0; i < PluginsList.Items.Count; i++)
            {
                var pluginsListItem = PluginsList.Items[i];
                plugins[i].Selected = pluginsListItem.Selected;
            }
        }
    }
}