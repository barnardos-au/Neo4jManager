using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
                .Append(" -DworkingDir=")
                .Append(quotes)
                .Append(neo4jHomeFolder)
                .Append(quotes);

            builder
                .Append(" -Djava.util.logging.config.file=")
                .Append(quotes)
                .Append($@"{neo4jHomeFolder}\conf\windows-wrapper-logging.properties")
                .Append(quotes);

            builder
                .Append(" -DconfigFile=")
                .Append(quotes)
                .Append("conf/neo4j-wrapper.conf")
                .Append(quotes);

            builder
                .Append(" -DserverClasspath=")
                .Append(quotes)
                .Append(@"lib/*.jar;system/lib/*.jar;plugins/**/*.jar;./conf*")
                .Append(quotes);

            builder
                .Append(" -DserverMainClass=")
                .Append(quotes)
                .Append("org.neo4j.server.CommunityBootstrapper")
                .Append(quotes);

            builder
                .Append(" -jar ")
                .Append(quotes)
                .Append($@"{neo4jHomeFolder}\bin\windows-service-wrapper-5.jar")
                .Append(quotes);

            return builder.ToString();
        }
    }
}
