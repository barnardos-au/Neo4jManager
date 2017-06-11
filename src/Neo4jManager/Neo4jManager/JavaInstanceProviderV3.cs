using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Neo4jManager
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class JavaInstanceProviderV3 : INeo4jInstanceProvider
    {
        private const string defaultDataDirectory = "data/databases";
        private const string defaultActiveDatabase = "graph.db";

        private readonly string neo4jHomeFolder;
        private readonly JavaProcessBuilderV3 javaProcessBuilder;
        private readonly ConfigEditor configEditor;

        private Process process;

        public JavaInstanceProviderV3(string javaPath, string neo4jHomeFolder, Neo4jEndpoints endpoints)
        {
            this.neo4jHomeFolder = neo4jHomeFolder;

            var configFile = Path.Combine(neo4jHomeFolder, "conf/neo4j.conf");
            configEditor = new ConfigEditor(configFile);

            javaProcessBuilder = new JavaProcessBuilderV3(javaPath, neo4jHomeFolder, configEditor);
            Endpoints = endpoints;
        }

        public void Start()
        {
            if (process == null)
            {
                process = javaProcessBuilder.GetProcess();
                process.Start();
                return;
            }

            if (process.HasExited)
                process.Start();
        }

        public void Stop()
        {
            if (process == null || process.HasExited) return;

            process.Kill();
        }

        public void Configure(string key, string value)
        {
            configEditor.SetValue(key, value);
        }

        public void Clear()
        {
            var dataDirectory = configEditor.GetValue("dbms.directories.data");
            if (string.IsNullOrEmpty(dataDirectory))
                dataDirectory = defaultDataDirectory;

            var activeDatabase = configEditor.GetValue("dbms.active_database");
            if (string.IsNullOrEmpty(activeDatabase))
                activeDatabase = defaultActiveDatabase;

            var dataPath = Path.Combine(neo4jHomeFolder, dataDirectory, activeDatabase);

            Stop();
            Directory.Delete(dataPath);
            Start();
        }

        public Neo4jEndpoints Endpoints { get; }

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
