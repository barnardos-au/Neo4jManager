using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;

namespace Neo4jManager
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class PowerShellInstanceProvider : Neo4jProcessBasedInstanceProvider, INeo4jInstanceProvider
    {
        private const int defaultWaitForKill = 10000;

        private Process process;
     
        public PowerShellInstanceProvider(string neo4jHomeFolder, IFileCopy fileCopy, Neo4jEndpoints endpoints)
            :base(neo4jHomeFolder, fileCopy, endpoints)
        {
        }

        public override async Task Start()
        {
            if (process == null)
            {
                process = GetProcess();
                process.Start();
                await this.WaitForReady();

                return;
            }

            if (!process.HasExited) return;

            process.Start();
            await this.WaitForReady();
        }

        public override async Task Stop()
        {
            if (process == null || process.HasExited) return;

            await Task.Run(() =>
            {
                process.Kill();
                process.WaitForExit(defaultWaitForKill);
            });
        }

        public void Dispose()
        {
            Stop().Wait();

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
