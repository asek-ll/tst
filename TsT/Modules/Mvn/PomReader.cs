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

namespace TsT.Modules.Mvn
{
    public class PomReader : IModule
    {
        private readonly MainForm _mainForm;
        private readonly Config _config;
        private readonly PluginModuleManager _pluginModuleManager;
        private readonly Logger _logger;
        private ParentProject _parentProject;

        private readonly string _mavenExecutable;

        public PomReader(MainForm mainForm, Config config, PluginModuleManager pluginModuleManager, Logger logger)
        {
            _mainForm = mainForm;
            _config = config;
            _pluginModuleManager = pluginModuleManager;
            _logger = logger;

            _mavenExecutable = config.GetPropertyOrDefault("maven", "mvn.cmd");
        }

        public void OnInit()
        {
            _mainForm.AddAction("Buld", async () => await Build(_pluginModuleManager.GetSelectedPlugins()));

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

            var addPluginMenuItem = new ToolStripMenuItem
            {
                //Name = "addPluginMenuItem",
                Size = new Size(100, 22),
                Text = "Add Plugin"
            };

            addPluginMenuItem.Click += AddPluginMenuItemClick;

            _mainForm.fileToolStripMenuItem.DropDownItems.Add(addPluginMenuItem);

            var removePluginItem = new ToolStripMenuItem
            {
                Text = "Remove Plugin"
            };
            removePluginItem.Click += RemovePlugin;

            _mainForm.contextMenu.Items.Add(removePluginItem);
        }

        public void OnDestroy()
        {
        }

        private void RemovePlugin(object sender, EventArgs e)
        {
            var file = GetLastOpenedFile();

            if (file == null)
            {
                return;
            }

            var plugins = _pluginModuleManager.GetSelectedPlugins();
            foreach (var plugin in plugins)
            {
                _parentProject.RemoveModule(plugin.RelativePath);
            }

            _parentProject.Save();
            OpenPomFile(file.FullName);
        }

        private void OpenPomFile(string filename)
        {
            _mainForm.Text = filename;
            _parentProject = new ParentProject(filename);


            var plugins = new List<PluginModule>(_parentProject.GetModules());

            _pluginModuleManager.Plugins = plugins.OrderBy(p => p.Name).ToList();

            foreach (var plugin in _pluginModuleManager.Plugins)
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
        private void AddPluginMenuItemClick(object sender, EventArgs e)
        {
            var file = GetUserSelectedPomFile();
            var lastFileInfo = GetLastOpenedFile();
            if (file == null || lastFileInfo == null)
            {
                return;
            }

            var pluginPomFileInfo = new FileInfo(file);
            var pluginPomUri = new Uri(pluginPomFileInfo.Directory.FullName);
            var projectPomUri = new Uri(lastFileInfo.FullName);

            var relativePath = projectPomUri.MakeRelativeUri(pluginPomUri).ToString();
            _parentProject.AddModule(relativePath);
            _parentProject.Save();
            OpenPomFile(lastFileInfo.FullName);
        }

        private string GetUserSelectedPomFile()
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

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                return openFileDialog.FileName;
            }
            return null;
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

        public async Task<bool> Build(IEnumerable<PluginModule> plugins, bool skipTests = false)
        {
            if (_parentProject == null) return false;

            var keys = plugins.Select(i => i.RelativePath).Where(i => i != null);
            var args = "install --projects " + string.Join(",", keys);

            if (skipTests)
            {
                args += " -Dmaven.test.skip=true";
            }

            _logger.Log("Start maven with args " + args);

            var exitCode = await RunMavenWithArgs(_parentProject.PomWrapper, args);

            if (exitCode != 0)
            {
                _logger.Error("ExitCode = " + exitCode);
            }
            else
            {
                _logger.Success("ExitCode = " + exitCode);
            }

            return exitCode == 0;
        }

        public async Task<int> RunMavenWithArgs(PomWrapper pomWrapper, string args)
        {
            if (pomWrapper.PomFileInfo.Directory == null) return -1;

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _mavenExecutable,
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