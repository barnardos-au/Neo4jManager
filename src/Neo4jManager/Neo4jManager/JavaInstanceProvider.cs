using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Neo4jManager
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class JavaInstanceProvider : INeo4jInstanceProvider
    {
        private readonly string javaPath;
        private readonly string neo4JHomeFolder;
        private readonly Neo4jOptions options;
        private const string quotes = "\"";

        private Process process;

        public JavaInstanceProvider(string javaPath, string neo4jHomeFolder, Neo4jOptions options)
        {
            this.javaPath = javaPath;
            neo4JHomeFolder = neo4jHomeFolder;
            this.options = options;
        }

        public void Start()
        {
            var currentProcess = GetProcess();
            currentProcess.Start();
        }

        public void Stop()
        {
            var currentProcess = GetProcess();
            currentProcess.Kill();
        }

        private Process GetProcess()
        {
            if (process != null) return process;

            process = new Process {StartInfo = GetProcessStartInfo()};
            return process;
        }

        private ProcessStartInfo GetProcessStartInfo()
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = javaPath,
                Arguments = GetJavaCmdArguments()
            };

            return processStartInfo;
        }

        private string GetJavaCmdArguments()
        {
            AddDefaultParameters();

            var builder = new StringBuilder();

            builder
                .Append(" -cp ")
                .Append(quotes)
                .Append($"{neo4JHomeFolder}/lib/*;{neo4JHomeFolder}/plugins/*")
                .Append(quotes);
            builder.Append(" -server");
            builder.Append(" -XX:+UseG1GC");
            builder.Append(" -XX:-OmitStackTraceInFastThrow");
            builder.Append(" -XX:+AlwaysPreTouch");
            builder.Append(" -XX:+UnlockExperimentalVMOptions");
            builder.Append(" -XX:+TrustFinalNonStaticFields");
            builder.Append(" -XX:+DisableExplicitGC");

            if (!string.IsNullOrEmpty(options.HeapInitialSize))
            {
                builder.Append($" -Xms{options.HeapInitialSize}");
            }
            if (!string.IsNullOrEmpty(options.HeapMaxSize))
            {
                builder.Append($" -Xmx{options.HeapMaxSize}");
            }
            foreach (var optionsParameter in options.Parameters)
            {
                builder.Append($" -D{optionsParameter.Key}={optionsParameter.Value}");
            }

            builder
                .Append(" org.neo4j.server.CommunityEntryPoint")
                .Append(" --config-dir=")
                .Append(quotes)
                .Append($@"{neo4JHomeFolder}\conf")
                .Append(quotes)
                .Append(" --home-dir=")
                .Append(quotes)
                .Append(neo4JHomeFolder)
                .Append(quotes);
                
            return builder.ToString();
        }

        private void AddDefaultParameters()
        {
            if (!options.Parameters.ContainsKey("log4j.configuration"))
            {
                options.Parameters.Add("log4j.configuration", "file:conf/log4j.properties");
            }

            if (!options.Parameters.ContainsKey("neo4j.ext.udc.source"))
            {
                options.Parameters.Add("neo4j.ext.udc.source", "zip-powershell");
            }

            if (!options.Parameters.ContainsKey("org.neo4j.cluster.logdirectory"))
            {
                options.Parameters.Add("org.neo4j.cluster.logdirectory", "data/log");
            }

            if (!options.Parameters.ContainsKey("jdk.tls.ephemeralDHKeySize"))
            {
                options.Parameters.Add("jdk.tls.ephemeralDHKeySize", "2048");
            }

            if (!options.Parameters.ContainsKey("unsupported.dbms.udc.source"))
            {
                options.Parameters.Add("unsupported.dbms.udc.source", "zip");
            }
        }

        public void Dispose()
        {
            if (process != null && !process.HasExited)
            {
                process.Kill();
            }

            process?.Dispose();
        }
    }
}
