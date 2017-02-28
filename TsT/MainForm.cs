using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using TsT.Entities;
using TsT.Components;
using System.Linq;
using System.ComponentModel;

namespace TsT
{
    public partial class MainForm : Form
    {
        private readonly Logger _logger;
        private readonly Config _config;
        private readonly JiraManager _jiraManager;

        private List<PluginModule> _plugins = new List<PluginModule>();

        private List<PluginModule> Plugins
        {
            set
            {
                _plugins = value;
                RenderPlugins();
            }
            get { return _plugins; }
        }

        public MainForm(Logger logger, Config config, JiraManager jiraManager)
        {
            InitializeComponent();

            _logger = logger;
            _config = config;
            _jiraManager = jiraManager;

            logger.OnLogging += (s, e) =>
            {
                LogView.AppendText(e.Message + Environment.NewLine);
                LogView.ScrollToCaret();
                //LogView.Refresh();
            };

            var lastOpenedFile = GetLastOpenedFile();

            if (lastOpenedFile != null)
            {
                OpenPomFile(lastOpenedFile.FullName);
            }
        }


        private void RenderPlugins()
        {
            PluginsList.Items.Clear();

            foreach (var plugin in _plugins)
            {
                var status = plugin.Enabled ? "ON" : "OFF";
                var item = new ListViewItem(new[] {plugin.Name, status}) {Name = "1"};

                PluginsList.Items.Add(item);
                plugin.PropertyChanged += PluginPropertyChanged;
            }
        }

        private void PluginPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var plugin = (PluginModule) sender;
            var index = _plugins.IndexOf(plugin);

            if (e.PropertyName != "enabled") return;

            var status = plugin.Enabled ? "ON" : "OFF";
            PluginsList.Items[index].SubItems[1].Text = status;
        }

        private void OpenPomFile(string filename)
        {
            Text = filename;
            var parentProject = new ParentProject(filename);

            Plugins = new List<PluginModule>(parentProject.GetModules());

            foreach (var plugin in Plugins)
            {
                plugin.GetBranch().ContinueWith(output =>
                {

                });
            }

            _jiraManager.GetEnabledPluginKeys()
                .ContinueWith(task =>
                    {
                        IEnumerable<string> enabledPluginKeys = task.Result;

                        foreach (var plugin in Plugins)
                        {
                            if (enabledPluginKeys.Contains(plugin.Key))
                            {
                                plugin.Enabled = true;
                            }
                        }
                    }
                );
        }

        private void SavePomFile(string filename)
        {
            _config.SetProperty("last.file.name", filename);
            _config.Store();
        }

        private FileInfo GetLastOpenedFile()
        {
            var lastOpenedFile = _config.GetProperty<string>("last.file.name");

            if (lastOpenedFile == null) return null;

            try
            {
                return new FileInfo(lastOpenedFile);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void OpenToolStripMenuItemClick(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog();

            var directory = "c:\\";

            var lastFileInfo = GetLastOpenedFile();
            if (lastFileInfo != null)
            {
                if (lastFileInfo.Directory != null) directory = lastFileInfo.Directory.FullName;
            }

            openFileDialog.InitialDirectory = directory;

            openFileDialog.Filter = "Poms|pom.xml";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() != DialogResult.OK) return;
            SavePomFile(openFileDialog.FileName);
            OpenPomFile(openFileDialog.FileName);
        }
    }
}