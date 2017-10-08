using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Neo4jManager.V3
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Neo4jV3ServiceInstanceProvider : Neo4jV3ProcessBasedInstanceProvider, INeo4jInstance
    {
        public Neo4jV3ServiceInstanceProvider(string neo4jHomeFolder, IFileCopy fileCopy, Neo4jVersion neo4jVersion, Neo4jEndpoints endpoints)
            : base(neo4jHomeFolder, fileCopy, neo4jVersion, endpoints)
        {
        }

        public override async Task Start(CancellationToken token)
        {
            await InstallService(token);
            await StartService(token);
        }

        public override async Task Stop(CancellationToken token)
        {
            await StopService(token);
            await UninstallService(token);
        }

        public override async Task Restart(CancellationToken token)
        {
            await Stop(token);
            await Start(token);
        }

        public Status Status { get; } = Status.Stopped;

        public void Dispose()
        {
            StopServiceProcess();
            UninstallServiceProcess();
        }

        private async Task InstallService(CancellationToken token)
        {
            await Task.Run(() =>
            {
                using (var process = GetProcess("install-service"))
                {
                    process.Start();
                    process.WaitForExit();
                }
            }, token);
        }

        private async Task StartService(CancellationToken token)
        {
            await Task.Run(() =>
            {
                using (var process = GetProcess("start"))
                {
                    process.Start();
                    process.WaitForExit();
                }
            }, token);

            await this.WaitForReady(token);
        }

        private async Task StopService(CancellationToken token)
        {
            await Task.Run(() =>
            {
                StopServiceProcess();
            }, token);
        }

        private void StopServiceProcess()
        {
            using (var process = GetProcess("stop"))
            {
                process.Start();
                process.WaitForExit();
            }
        }

        private async Task UninstallService(CancellationToken token)
        {
            await Task.Run(() =>
            {
                UninstallServiceProcess();
            }, token);
        }

        private void UninstallServiceProcess()
        {
            using (var process = GetProcess("uninstall-service"))
            {
                process.Start();
                process.WaitForExit();
            }
        }

        protected override async Task StopWhile(CancellationToken token, Action action)
        {
            await StopService(token);
            await Task.Run(action, token);
            await StartService(token);
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
