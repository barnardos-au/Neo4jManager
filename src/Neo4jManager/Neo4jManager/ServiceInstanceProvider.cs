using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;

namespace Neo4jManager
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ServiceInstanceProvider : Neo4jProcessBasedInstanceProvider, INeo4jInstanceProvider
    {
        public ServiceInstanceProvider(string neo4jHomeFolder, IFileCopy fileCopy, Neo4jEndpoints endpoints)
            :base(neo4jHomeFolder, fileCopy, endpoints)
        {
        }

        public override async Task Start()
        {
            await InstallService();
            await StartService();
        }

        public override async Task Stop()
        {
            await StopService();
            await UninstallService();
        }

        public override async Task Restart()
        {
            await StopService();
            await StartService();
        }

        public void Dispose()
        {
            Stop().Wait();
        }

        private async Task InstallService()
        {
            await Task.Run(() =>
            {
                using (var process = GetProcess("install-service"))
                {
                    process.Start();
                    process.WaitForExit();
                }
            });
        }

        private async Task StartService()
        {
            await Task.Run(() =>
            {
                using (var process = GetProcess("start"))
                {
                    process.Start();
                    process.WaitForExit();
                }
            });

            await this.WaitForReady();
        }

        private async Task StopService()
        {
            await Task.Run(() =>
            {
                using (var process = GetProcess("stop"))
                {
                    process.Start();
                    process.WaitForExit();
                }
            });
        }

        private async Task UninstallService()
        {
            await Task.Run(() =>
            {
                using (var process = GetProcess("uninstall-service"))
                {
                    process.Start();
                    process.WaitForExit();
                }
            });
        }

        protected override async Task StopWhile(Action action)
        {
            await StopService();
            await Task.Run(action);
            await StartService();
        }

        private Process GetProcess(string command)
        {
            return new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "POWERSHELL.EXE",
                    Arguments = GetPowerShellArguments(command)
                }
            };
        }

        private string GetPowerShellArguments(string command)
        {
            var builder = new StringBuilder();

            builder
                .Append(" -NoProfile")
                .Append(" -NonInteractive")
                .Append(" -NoLogo")
                .Append(" -ExecutionPolicy Bypass")
                .Append(" -Command ")
                .Append(quotes)
                .Append($@"Import-Module '{neo4jHomeFolder}\bin\Neo4j-Management.psd1'; Exit (Invoke-Neo4j {command})")
                .Append(quotes);

            return builder.ToString();
        }
    }
}
