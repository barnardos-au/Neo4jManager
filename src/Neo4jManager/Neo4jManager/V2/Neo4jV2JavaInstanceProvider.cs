using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Neo4jManager.V2
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Neo4jV2JavaInstanceProvider : Neo4jV2ProcessBasedInstanceProvider, INeo4jInstance
    {
        private const int defaultWaitForKill = 10000;

        private readonly string javaPath;

        private Process process;


        public Neo4jV2JavaInstanceProvider(string javaPath, string neo4jHomeFolder, IFileCopy fileCopy, Neo4jEndpoints endpoints)
            :base(neo4jHomeFolder, fileCopy, endpoints)
        {
            this.javaPath = javaPath;
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
                    FileName = javaPath,
                    Arguments = GetJavaCmdArguments()
                }
            };
        }

        private string GetJavaCmdArguments()
        {
            var builder = new StringBuilder();

            builder
                .Append(" -cp ")
                .Append(quotes)
                .Append($"{neo4jHomeFolder}/lib/*;{neo4jHomeFolder}/plugins/*")
                .Append(quotes);

            builder.Append(" -server");

            builder.Append(" -Dneo4j.ext.udc.source=zip-powershell");
            builder.Append(" -Dorg.neo4j.cluster.logdirectory=data/log");

            var jvmAdditionalParams = configEditors[Neo4jWrapperConfigFile]
                .FindValues("wrapper.java.additional")
                .Select(p => p.Value);

            foreach (var param in jvmAdditionalParams)
            {
                builder.Append($" {param}");
            }

            var heapInitialSize = configEditors[Neo4jWrapperConfigFile].GetValue("wrapper.java.initmemory");
            if (!string.IsNullOrEmpty(heapInitialSize))
            {
                builder.Append($" -Xms{heapInitialSize}");
            }
            var heapMaxSize = configEditors[Neo4jWrapperConfigFile].GetValue("wrapper.java.maxmemory");
            if (!string.IsNullOrEmpty(heapMaxSize))
            {
                builder.Append($" -Xmx{heapMaxSize}");
            }

            builder
                .Append(" org.neo4j.server.CommunityEntryPoint")
                .Append(" --config-dir=")
                .Append(quotes)
                .Append($@"{neo4jHomeFolder}\conf")
                .Append(quotes)
                .Append(" --home-dir=")
                .Append(quotes)
                .Append(neo4jHomeFolder)
                .Append(quotes);

            return builder.ToString();
        }
    }
}
