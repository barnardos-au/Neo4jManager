using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Neo4jManager
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ServiceInstanceProvider : INeo4jInstanceProvider
    {
        private const string quotes = "\"";

        private const string defaultDataDirectory = "data/databases";
        private const string defaultActiveDatabase = "graph.db";

        private readonly string neo4jHomeFolder;
        private readonly IFileCopy fileCopy;
        private readonly ConfigEditor configEditor;

        public ServiceInstanceProvider(string neo4jHomeFolder, Neo4jEndpoints endpoints, IFileCopy fileCopy)
        {
            this.neo4jHomeFolder = neo4jHomeFolder;
            this.fileCopy = fileCopy;

            var configFile = Path.Combine(neo4jHomeFolder, "conf/neo4j.conf");
            configEditor = new ConfigEditor(configFile);

            Endpoints = endpoints;
        }

        public async Task Start()
        {
            await Task.Run(() =>
            {
                using (var process = GetProcess("install-service"))
                {
                    process.Start();
                    process.WaitForExit();
                }

                using (var process = GetProcess("start"))
                {
                    process.Start();
                    process.WaitForExit();
                }
            });

            await this.WaitForReady();
        }

        public async Task Stop()
        {
            await Task.Run(() =>
            {
                using (var process = GetProcess("stop"))
                {
                    process.Start();
                    process.WaitForExit();
                }

                using (var process = GetProcess("uninstall-service"))
                {
                    process.Start();
                    process.WaitForExit();
                }
            });
        }

        public void Configure(string key, string value)
        {
            configEditor.SetValue(key, value);
        }

        public async Task Clear()
        {
            var dataPath = GetDataPath();

            await Stop();
            Directory.Delete(dataPath);
            await Start();
        }

        public async Task Backup(string destinationPath, bool stopInstanceBeforeBackup = true)
        {
            var dataPath = GetDataPath();

            if (stopInstanceBeforeBackup) await Stop();
            fileCopy.MirrorFolders(dataPath, destinationPath);
            if (stopInstanceBeforeBackup) await Start();
        }

        public async Task Restore(string sourcePath)
        {
            var dataPath = GetDataPath();

            await Stop();
            fileCopy.MirrorFolders(sourcePath, dataPath);
            await Start();
        }

        public Neo4jEndpoints Endpoints { get; }

        public void Dispose()
        {
            Stop().Wait();
        }

        private string GetDataPath()
        {
            var dataDirectory = configEditor.GetValue("dbms.directories.data");
            if (string.IsNullOrEmpty(dataDirectory))
                dataDirectory = defaultDataDirectory;

            var activeDatabase = configEditor.GetValue("dbms.active_database");
            if (string.IsNullOrEmpty(activeDatabase))
                activeDatabase = defaultActiveDatabase;

            return Path.Combine(neo4jHomeFolder, dataDirectory, activeDatabase);
        }

        public Process GetProcess(string command)
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
