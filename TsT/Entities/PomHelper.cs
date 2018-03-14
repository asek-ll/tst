using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TsT.Entities
{
    public class PomWrapper
    {
        private readonly XElement _projectElement;
        private readonly XNamespace _ns = "http://maven.apache.org/POM/4.0.0";
        public readonly FileInfo PomFileInfo;
        private readonly XDocument _document;

        public PomWrapper(string filePath)
        {
            PomFileInfo = new FileInfo(filePath);
            _document = XDocument.Parse(File.ReadAllText(filePath));
            _projectElement = _document.Root;
        }

        public string GetArtifactId()
        {
            var xElement = _projectElement.Element(_ns + "artifactId");
            return xElement != null ? xElement.Value : null;
        }

        public string GetGroupId()
        {
            var groupElement = _projectElement.Element(_ns + "groupId");
            if (groupElement != null)
            {
                return groupElement.Value;
            }
            var parentElement = GetParentElement();
            if (parentElement == null) return "";

            var parentGroup = parentElement.Element(_ns + "groupId");
            return parentGroup != null ? parentGroup.Value : "";
        }

        private XElement GetParentElement()
        {
            return _projectElement.Element(_ns + "parent");
        }

        private IEnumerable<string> GetModules(XContainer element)
        {
            var modulePathes = new List<string>();

            var modules = element.Element(_ns + "modules");
            if (modules != null)
            {
                modulePathes.AddRange(modules.Elements(_ns + "module").Select(module => module.FirstNode.ToString()));
            }

            return modulePathes;
        }

        public void RemoveModule(string modulePath)
        {
            var modules = _projectElement.Element(_ns + "modules");

            if (modules == null)
            {
                var profiles = _projectElement.Element(_ns + "profiles");

                if (profiles != null)
                {
                    var firstProfile = profiles.Elements(_ns + "profile").First();
                    if (firstProfile != null)
                    {

                        modules = firstProfile.Element(_ns + "modules");
                    }
                }


            }

            if (modules != null)
            {
                foreach(var module in modules.Elements(_ns + "module"))
                {
                    var content = module.FirstNode.ToString();
                    if (content == modulePath)
                    {
                        module.Remove();
                    }
                }
            }
        }

        public IEnumerable<string> GetModules()
        {
            var modulePathes = new List<string>();

            modulePathes.AddRange(GetModules(_projectElement));

            var profiles = _projectElement.Element(_ns + "profiles");

            if (profiles == null) return modulePathes;
            foreach (var profile in profiles.Elements(_ns + "profile"))
            {
                modulePathes.AddRange(GetModules(profile));
            }

            return modulePathes;
        }

        public void AddModule(string module)
        {
            var modules = _projectElement.Element(_ns + "modules");

            if (modules == null)
            {
                var profiles = _projectElement.Element(_ns + "profiles");

                if (profiles != null)
                {
                    var firstProfile = profiles.Elements(_ns + "profile").First();
                    if (firstProfile != null)
                    {

                        modules = firstProfile.Element(_ns + "modules");
                    }
                }


            }

            if (modules != null)
            {
                var newModule = new XElement(_ns + "module");
                newModule.AddFirst(module);
                modules.Add(newModule);
            }
        }

        public void Save(string filename)
        {
            _document.Save(filename);
        }

    }
}