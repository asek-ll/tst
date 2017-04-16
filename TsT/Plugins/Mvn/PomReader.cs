using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TsT.Components;
using TsT.Entities;

namespace TsT.Plugins.Mvn
{
    public class PomReader : IPlugin
    {
        private readonly MainForm _mainForm;
        private readonly Config _config;
        private readonly PluginManager _pluginManager;
        private readonly Logger _logger;
        private ParentProject _parentProject;

        public PomReader(MainForm mainForm, Config config, PluginManager pluginManager, Logger logger)
        {
            _mainForm = mainForm;
            _config = config;
            _pluginManager = pluginManager;
            _logger = logger;
        }

        public void OnInit()
        {
            _mainForm.AddAction("Buld", Build);

            var openToolStripMenuItem = new ToolStripMenuItem
            {
                Name = "openToolStripMenuItem",
                Size = new Size(100, 22),
                Text = "Open"
            };
            openToolStripMenuItem.Click += OpenToolStripMenuItemClick;

            _mainForm.fileToolStripMenuItem.DropDownItems.Add(openToolStripMenuItem);

            var lastOpenedFile = GetLastOpenedFile();

            if (lastOpenedFile != null)
            {
                OpenPomFile(lastOpenedFile.FullName);
            }
        }

        public void OnDestroy()
        {
        }

        private void OpenPomFile(string filename)
        {
            _mainForm.Text = filename;
            _parentProject = new ParentProject(filename);

            _pluginManager.Plugins = new List<PluginModule>(_parentProject.GetModules());

            foreach (var plugin in _pluginManager.Plugins)
            {
                var plugin1 = plugin;
                plugin.GetBranch().ContinueWith(output => { plugin1.Branch = output.Result; });
            }
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
            openFileDialog.Filter = @"Poms|pom.xml";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() != DialogResult.OK) return;
            SavePomFile(openFileDialog.FileName);
            OpenPomFile(openFileDialog.FileName);
        }

        private void Build()
        {
            if (_parentProject == null) return;

            var keys = _mainForm.GetSelectedPlugins().Select(i => i.RelativePath).Where(i => i != null);
            var args = "install --projects " + string.Join(",", keys);

            _logger.Log("Start maven with args " + args);

            RunMavenWithArgs(_parentProject.PomWrapper, args)
                .ContinueWith(runTask => _logger.Log("ExitCode = " + runTask.Result));
        }

        public async Task<int> RunMavenWithArgs(PomWrapper pomWrapper, string args)
        {
            if (pomWrapper.PomFileInfo.Directory == null) return -1;

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "mvn.bat",
                    Arguments = args,
//                    UseShellExecute = false,
//                    RedirectStandardOutput = true,
//                    CreateNoWindow = true,
                    WorkingDirectory = pomWrapper.PomFileInfo.Directory.FullName
                }
            };

            return await RunProcessAsync(proc);
        }

        private static Task<int> RunProcessAsync(Process process)
        {
            var tcs = new TaskCompletionSource<int>();

            process.EnableRaisingEvents = true;

            process.Exited +=
                (sender, args) =>
                {
                    tcs.SetResult(process.ExitCode);
                    process.Dispose();
                };
            process.Start();
            return tcs.Task;
        }
    }
}