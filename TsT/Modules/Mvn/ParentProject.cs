using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TsT.Entities;

namespace TsT.Modules.Mvn
{
    public class ParentProject
    {
        private readonly string _pomFilePath;
        public readonly PomWrapper PomWrapper;
        private readonly string _pomFolder;
        private readonly Dictionary<PluginModule, string> _pluginToModulePath = new Dictionary<PluginModule, string>();

        public ParentProject(string pomFilePath)
        {
            this._pomFilePath = pomFilePath;
            PomWrapper = new PomWrapper(pomFilePath);

            var fileInfo = new FileInfo(pomFilePath);
            if (fileInfo.Directory != null) _pomFolder = fileInfo.Directory.FullName;

            InitModules();
        }

        private void InitModules()
        {
            foreach (var module in PomWrapper.GetModules())
            {
                var transasPlugin =
                    new PluginModule(Path.GetFullPath(Path.Combine(_pomFolder, module))) { RelativePath = module };

                _pluginToModulePath.Add(transasPlugin, module);
            }
        }

        public IEnumerable<PluginModule> GetModules()
        {
            return _pluginToModulePath.Keys;
        }

        public void AddModule(string modulePath)
        {
            if (!Enumerable.Contains(PomWrapper.GetModules(), modulePath))
            {
                PomWrapper.AddModule(modulePath);
            }
        }


        public void Save()
        {
            PomWrapper.Save(_pomFilePath);
        }

        public void RemoveModule(string modulePath)
        {
            if (Enumerable.Contains(PomWrapper.GetModules(), modulePath))
            {
                PomWrapper.RemoveModule(modulePath);
            }
        }
    }
}