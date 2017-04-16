using System.Collections.Generic;
using System.IO;
using TsT.Entities;

namespace TsT.Plugins.Mvn
{
    public class ParentProject
    {
        public readonly PomWrapper PomWrapper;
        private readonly string _pomFolder;
        private readonly Dictionary<PluginModule, string> _pluginToModulePath = new Dictionary<PluginModule, string>();

        public ParentProject(string pomFilePath)
        {
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
                    new PluginModule(Path.GetFullPath(Path.Combine(_pomFolder, module))) {RelativePath = module};

                _pluginToModulePath.Add(transasPlugin, module);
            }
        }

        public IEnumerable<PluginModule> GetModules()
        {
            return _pluginToModulePath.Keys;
        }
    }
}