using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using System.Threading.Tasks;

namespace TsT.Entities
{
    public class PluginModule : INotifyPropertyChanged
    {
        public readonly string Name;
        public readonly string PomPath;
        private readonly PomWrapper _pomWrapper;
        private bool _enabled = false;
        private string _branch = "";

        public string RelativePath { get; set; }

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                OnPropertyChanged("enabled");
            }
        }

        public string Branch
        {
            get { return _branch; }
            set
            {
                _branch = value;
                OnPropertyChanged("branch");
            }
        }

        public string Key
        {
            get { return _pomWrapper.GetGroupId() + "." + _pomWrapper.GetArtifactId(); }
        }

        public PluginModule(string path)
        {
            PomPath = path;

            _pomWrapper = new PomWrapper(Path.Combine(PomPath, "pom.xml"));
            Name = _pomWrapper.GetArtifactId();
        }

        public async Task<string> GetBranch()
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git.exe",
                    Arguments = "rev-parse --abbrev-ref HEAD",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WorkingDirectory = PomPath
                }
            };

            proc.Start();
            return await proc.StandardOutput.ReadToEndAsync();
        }

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}