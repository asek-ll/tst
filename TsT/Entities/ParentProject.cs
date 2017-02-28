using System;
using System.IO;
using System.Collections.Generic;

namespace TsT.Entities
{
    public class ParentProject
    {
        private readonly PomWrapper _pomWrapper;
        private readonly string _pomFolder;
        private readonly Dictionary<PluginModule, string> _pluginToModulePath = new Dictionary<PluginModule, string>();

        public ParentProject(string pomFilePath)
        {
            _pomWrapper = new PomWrapper(pomFilePath);

            var fileInfo = new FileInfo(pomFilePath);
            if (fileInfo.Directory != null) _pomFolder = fileInfo.Directory.FullName;

            InitModules();
        }

        private void InitModules()
        {
            foreach (var module in _pomWrapper.GetModules())
            {
                var transasPlugin = new PluginModule(Path.GetFullPath(Path.Combine(_pomFolder, module)));

                _pluginToModulePath.Add(transasPlugin, module);
            }
        }

        public IEnumerable<PluginModule> GetModules()
        {
            return _pluginToModulePath.Keys;
        }
    }
}