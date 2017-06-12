using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace Neo4jManager
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public abstract class Neo4jProcessBasedInstanceProvider
    {
        protected const string quotes = "\"";

        protected const string defaultDataDirectory = "data/databases";
        protected const string defaultActiveDatabase = "graph.db";

        protected readonly string neo4jHomeFolder;
        private readonly IFileCopy fileCopy;
        protected readonly ConfigEditor configEditor;

        protected Neo4jProcessBasedInstanceProvider(string neo4jHomeFolder, IFileCopy fileCopy, Neo4jEndpoints endpoints)
        {
            this.neo4jHomeFolder = neo4jHomeFolder;
            this.fileCopy = fileCopy;
            Endpoints = endpoints;

            var configFile = Path.Combine(neo4jHomeFolder, "conf/neo4j.conf");
            configEditor = new ConfigEditor(configFile);
        }

        public abstract Task Stop();

        public abstract Task Start();

        public virtual async Task Restart()
        {
            await Stop();
            await Start();
        }

        public virtual async Task Clear()
        {
            var dataPath = GetDataPath();

            await StopWhile(() => Directory.Delete(dataPath));
        }

        public virtual async Task Backup(string destinationPath, bool stopInstanceBeforeBackup = true)
        {
            var dataPath = GetDataPath();

            var action = new Action(() => fileCopy.MirrorFolders(dataPath, destinationPath));

            if (stopInstanceBeforeBackup)
            {
                await StopWhile(action);
            }
            else
            {
                await Task.Run(action);
            }
        }

        public virtual async Task Restore(string sourcePath)
        {
            var dataPath = GetDataPath();

            await StopWhile(() => fileCopy.MirrorFolders(sourcePath, dataPath));
        }

        public virtual void Configure(string key, string value)
        {
            configEditor.SetValue(key, value);
        }

        public virtual Neo4jEndpoints Endpoints { get; }

        protected virtual async Task StopWhile(Action action)
        {
            await Stop();
            await Task.Run(action);
            await Start();
        }

        protected virtual string GetDataPath()
        {
            var dataDirectory = configEditor.GetValue("dbms.directories.data");
            if (string.IsNullOrEmpty(dataDirectory))
                dataDirectory = defaultDataDirectory;

            var activeDatabase = configEditor.GetValue("dbms.active_database");
            if (string.IsNullOrEmpty(activeDatabase))
                activeDatabase = defaultActiveDatabase;

            return Path.Combine(neo4jHomeFolder, dataDirectory, activeDatabase);
        }
    }
}
