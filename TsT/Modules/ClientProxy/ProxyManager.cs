using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using TsT.Components;

namespace TsT.Modules.ClientProxy
{
    public class ProxyManager
    {
        private readonly string _port;
        private readonly string _adress;
        private readonly string _yaxyConfig;
        private readonly string _yaxyLocation;
        private Process _yaxyProcess;
        private bool _proxyEnabled;
        private bool _globalEnabled;
        private readonly Logger _logger;

        public string Port
        {
            get { return _port ?? "8558"; }
        }

        public string Adress
        {
            get { return _adress ?? "127.0.0.1"; }
        }

        public string YaxyConfig
        {
            get { return _yaxyConfig ?? ""; }
        }

        public ProxyManager(Config config, Logger logger)
        {
            _port = config.GetProperty<string>("proxy.port");
            _adress = config.GetProperty<string>("proxy.ip");
            _yaxyConfig = config.GetProperty<string>("yaxy.config");
            _yaxyLocation = config.GetProperty<string>("yaxy.location");

            _logger = logger;
        }

        [DllImport("wininet.dll", SetLastError = true)]
        private static extern bool InternetSetOption(
            IntPtr hInternet,
            int dwOption,
            IntPtr lpBuffer,
            int lpdwBufferLength);

        private void SetGlobalProxy()
        {
            var internetSettingsKey = GetInternetSettingsKey();

            if (internetSettingsKey == null) return;

            internetSettingsKey.SetValue("ProxyEnable", "1", RegistryValueKind.DWord);
            internetSettingsKey.SetValue("ProxyServer", Adress + ":" + Port, RegistryValueKind.String);
            InternetSetOption(IntPtr.Zero, 39, IntPtr.Zero, 0);
            _logger.Log(string.Format("Global proxy enabled on {0}:{1}", Adress, Port));
        }

        private void StartYaxy()
        {
            if (_yaxyLocation == null)
            {
                throw new ArgumentException();
            }

            var yaxyArgs = "--port " + Port + " --proxy " + Adress;

            if (YaxyConfig != "")
            {
                yaxyArgs += " --config " + YaxyConfig;
            }

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "node.exe",
                    Arguments = "node_modules/yaxy/bin/proxy.js " + yaxyArgs,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = _yaxyLocation
                }
            };

            proc.Start();

            _logger.Log(string.Format("Yaxy started with args {0} from {1}", yaxyArgs, _yaxyLocation));

            _yaxyProcess = proc;
        }

        private void StopYaxy()
        {
            if (_yaxyProcess != null && !_yaxyProcess.HasExited)
            {
                _yaxyProcess.Kill();
            }
            _yaxyProcess = null;
            _logger.Log("Yaxy stopped");
        }

        private void RemoveGlobalProxy()
        {
            var internetSettingsKey = GetInternetSettingsKey();

            if (internetSettingsKey == null) return;

            internetSettingsKey.SetValue("ProxyEnable", "0", RegistryValueKind.DWord);
            InternetSetOption(IntPtr.Zero, 39, IntPtr.Zero, 0);

            _logger.Log("Disable global proxy");
        }

        private static RegistryKey GetInternetSettingsKey()
        {
            var openSubKey = Registry.CurrentUser.OpenSubKey("Software", true);
            if (openSubKey == null) return null;

            var registryKey = openSubKey.OpenSubKey("Microsoft", true);

            if (registryKey == null) return null;

            var subKey = registryKey.OpenSubKey("Windows", true);

            if (subKey == null) return null;

            var key = subKey.OpenSubKey("CurrentVersion", true);

            return key != null ? key.OpenSubKey("Internet Settings", true) : null;
        }

        public void SetProxyEnabled(bool state)
        {
            if (state == _proxyEnabled) return;

            if (state)
            {
                StartYaxy();
                if (_globalEnabled)
                {
                    SetGlobalProxy();
                }
            }
            else
            {
                StopYaxy();
                if (_globalEnabled)
                {
                    RemoveGlobalProxy();
                }
            }

            _proxyEnabled = state;
        }

        public void SetProxyGlobalEnabled(bool state)
        {
            if (state == _globalEnabled) return;

            if (_proxyEnabled)
            {
                if (state)
                {
                    SetGlobalProxy();
                }
                else
                {
                    RemoveGlobalProxy();
                }
            }

            _globalEnabled = state;
        }
    }
}