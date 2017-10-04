using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Neo4jManager.V3
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Neo4jV3PowerShellInstanceProvider : Neo4jV3ProcessBasedInstanceProvider, INeo4jInstance
    {
        private const int defaultWaitForKill = 10000;

        private Process process;

        public Neo4jV3PowerShellInstanceProvider(string neo4jHomeFolder, IFileCopy fileCopy, Neo4jVersion neo4jVersion, Neo4jEndpoints endpoints)
            : base(neo4jHomeFolder, fileCopy, neo4jVersion, endpoints)
        {
        }

        public override async Task Start(CancellationToken token)
        {
            if (process == null)
            {
                process = GetProcess();
                process.Start();
                await this.WaitForReady(token);

                return;
            }

            if (!process.HasExited) return;

            process.Start();
            await this.WaitForReady(token);
        }

        public override async Task Stop(CancellationToken token)
        {
            if (process == null || process.HasExited) return;

            await Task.Run(() =>
            {
                Stop();
            }, token);
        }

        private void Stop()
        {
            process.Kill();
            process.WaitForExit(defaultWaitForKill);
        }

        public void Dispose()
        {
            Stop();

            process?.Dispose();
        }

        private Process GetProcess()
        {
            return new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "POWERSHELL.EXE",
                    Arguments = GetPowerShellArguments()
                }
            };
        }

        private string GetPowerShellArguments()
        {
            var builder = new StringBuilder();

            builder
                .Append(" -NoProfile")
                .Append(" -NonInteractive")
                .Append(" -NoLogo")
                .Append(" -ExecutionPolicy Bypass")
                .Append(" -Command ")
                .Append(quotes)
                .Append($@"Import-Module '{neo4jHomeFolder}\bin\Neo4j-Management.psd1'; Exit (Invoke-Neo4j Console)")
                .Append(quotes);

            return builder.ToString();
        }
    }
}
