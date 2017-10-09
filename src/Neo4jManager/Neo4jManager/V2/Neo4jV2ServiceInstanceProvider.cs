using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Neo4jManager.V2
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Neo4jV2ServiceInstanceProvider : Neo4jV2ProcessBasedInstanceProvider, INeo4jInstance
    {
        internal static class Neo4jV2ServiceController
        {
            private static readonly object mutex = new object();

            public static void InstallService(string neo4jHomeFolder, string serviceName)
            {
                using (var process = GetProcess($"Install-Neo4jServer -Name '{serviceName}'", neo4jHomeFolder))
                {
                    ProcessInvoke(process);
                }
            }

            public static void StartService(string neo4jHomeFolder)
            {
                using (var process = GetProcess("Start-Neo4jServer", neo4jHomeFolder))
                {
                    ProcessInvoke(process);
                }
            }

            public static void StopService(string neo4jHomeFolder)
            {
                using (var process = GetProcess("Stop-Neo4jServer", neo4jHomeFolder))
                {
                    ProcessInvoke(process);
                }
            }

            public static void UninstallService(string neo4jHomeFolder)
            {
                using (var process = GetProcess("Uninstall-Neo4jServer", neo4jHomeFolder))
                {
                    ProcessInvoke(process);
                }
            }

            private static void ProcessInvoke(Process process)
            {
                lock (mutex)
                {
                    process.Start();
                    var error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine(error);
                    }
                }
            }

            private static Process GetProcess(string command, string neo4jHomeFolder)
            {
                var args = new StringBuilder()
                    .Append(" -NoProfile")
                    .Append(" -NonInteractive")
                    .Append(" -NoLogo")
                    .Append(" -ExecutionPolicy Bypass")
                    .Append(" -Command ")
                    .Append(quotes)
                    .Append($@"Import-Module '{neo4jHomeFolder}\bin\Neo4j-Management.psd1'; ")
                    .Append("[System.Environment]::SetEnvironmentVariable('NEO4J_HOME', '', 'Process'); ")
                    .Append($"Exit ('{neo4jHomeFolder}' | {command})")
                    .Append(quotes)
                    .ToString();

                return new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "POWERSHELL.EXE",
                        Arguments = args,
                        UseShellExecute = false,
                        RedirectStandardError = true
                    }
                };
            }
        }

        public Neo4jV2ServiceInstanceProvider(string neo4jHomeFolder, IFileCopy fileCopy, Neo4jVersion neo4jVersion, Neo4jEndpoints endpoints)
            : base(neo4jHomeFolder, fileCopy, neo4jVersion, endpoints)
        {
        }

        public override async Task Start(CancellationToken token)
        {
            await InstallService(token);
            await StartService(token);
            Status = Status.Started;
        }

        public override async Task Stop(CancellationToken token)
        {
            await StopService(token);
            await UninstallService(token);
            Status = Status.Stopped;
        }

        public override async Task Restart(CancellationToken token)
        {
            await Stop(token);
            await Start(token);
        }

        public Status Status { get; private set; } = Status.Stopped;

        public void Dispose()
        {
            Neo4jV2ServiceController.StopService(neo4jHomeFolder);
            Neo4jV2ServiceController.UninstallService(neo4jHomeFolder);
        }

        private async Task InstallService(CancellationToken token)
        {
            var serviceName = configEditors[Neo4jWrapperConfigFile].GetValue("wrapper.name");

            await Task.Run(
                () => Neo4jV2ServiceController.InstallService(neo4jHomeFolder, serviceName), token);
        }

        private async Task StartService(CancellationToken token)
        {
            Status = Status.Starting;

            await Task.Run(
                () => Neo4jV2ServiceController.StartService(neo4jHomeFolder), token);
            await this.WaitForReady(token);
        }

        private async Task StopService(CancellationToken token)
        {
            Status = Status.Stopping;

            await Task.Run(
                () => Neo4jV2ServiceController.StopService(neo4jHomeFolder), token);
        }

        private async Task UninstallService(CancellationToken token)
        {
            await Task.Run(
                () => Neo4jV2ServiceController.UninstallService(neo4jHomeFolder), token);
        }

        protected override async Task StopWhile(CancellationToken token, Action action)
        {
            await StopService(token);
            await Task.Run(action, token);
            await StartService(token);
        }
    }
}
