using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Neo4jManager
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class JavaProcessBuilderV3
    {
        private const string quotes = "\"";

        private readonly string javaPath;
        private readonly string neo4jHomeFolder;
        private readonly ConfigEditor configEditor;

        public JavaProcessBuilderV3(string javaPath, string neo4jHomeFolder, ConfigEditor configEditor)
        {
            this.javaPath = javaPath;
            this.neo4jHomeFolder = neo4jHomeFolder;
            this.configEditor = configEditor;
        }

        public Process GetProcess()
        {
            return new Process { StartInfo = GetProcessStartInfo() };
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
            var builder = new StringBuilder();

            builder
                .Append(" -cp ")
                .Append(quotes)
                .Append($"{neo4jHomeFolder}/lib/*;{neo4jHomeFolder}/plugins/*")
                .Append(quotes);

            builder.Append(" -server");

            builder.Append(" -Dlog4j.configuration=file:conf/log4j.properties");
            builder.Append(" -Dneo4j.ext.udc.source=zip-powershell");
            builder.Append(" -Dorg.neo4j.cluster.logdirectory=data/log");

            var jvmAdditionalParams = configEditor
                .FindValues("dbms.jvm.additional")
                .Select(p => p.Value);

            foreach (var param in jvmAdditionalParams)
            {
                builder.Append($" {param}");
            }

            var heapInitialSize = configEditor.GetValue("dbms.memory.heap.initial_size");
            if (!string.IsNullOrEmpty(heapInitialSize))
            {
                builder.Append($" -Xms{heapInitialSize}");
            }
            var heapMaxSize = configEditor.GetValue("dbms.memory.heap.max_size");
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
