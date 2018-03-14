using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsT.Components
{
    public class Utils
    {
        private Logger _logger;
        private Config _config;

        public Utils(Logger logger, Config config)
        {
            _logger = logger;
            _config = config;
        }

        public static Task<int> RunProcessAsync(Process process)
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

        public async Task OpenDir(string path)
        {
            var fileManager = _config.GetProperty<string>("file.manager");

            if (fileManager == null)
            {
                _logger.Error("Error: specify file.manager property in config.json");
            }
            else
            {
                var arguments = _config.GetProperty<string>("file.manager.arguments");
                await ExecuteWithParam("\"" + path + "\"", fileManager, arguments);
            }
        }

        public async Task ExecuteWithParam(string path, string program, string arguments)
        {

            if (arguments != null)
            {
                var parts = arguments.Split('%');
                if (parts.Length > 1)
                {
                    arguments = parts[0] + path + parts[1];
                }
            }
            else
            {
                arguments = path;
            }

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = program,
                    Arguments = arguments,
                    WorkingDirectory = path,
                }
            };

            _logger.Log("Exec: " + program + " " + arguments);

            try
            {
                var code = await RunProcessAsync(proc);
            }
            catch (Exception e)
            {
                _logger.Error("Error on executing " + program + " " + arguments + ": " + e.Message);
            }

        }
    }
}
