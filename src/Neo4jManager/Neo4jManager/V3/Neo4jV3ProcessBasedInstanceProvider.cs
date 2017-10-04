using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Neo4jManager.V3
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public abstract class Neo4jV3ProcessBasedInstanceProvider
    {
        public const string Neo4jConfigFile = "neo4j.conf";

        protected const string quotes = "\"";

        protected const string defaultDataDirectory = "data/databases";
        protected const string defaultActiveDatabase = "graph.db";

        private readonly IFileCopy fileCopy;

        protected readonly string neo4jHomeFolder;
        protected readonly string neo4jConfigFolder;
        protected readonly Dictionary<string, ConfigEditor> configEditors;

        protected Neo4jV3ProcessBasedInstanceProvider(string neo4jHomeFolder, IFileCopy fileCopy, Neo4jVersion neo4jVersion, Neo4jEndpoints endpoints)
        {
            this.neo4jHomeFolder = neo4jHomeFolder;
            this.fileCopy = fileCopy;

            neo4jConfigFolder = Path.Combine(neo4jHomeFolder, "conf");

            Version = neo4jVersion;
            Endpoints = endpoints;

            configEditors = new Dictionary<string, ConfigEditor>
            {
                {
                    Neo4jConfigFile,
                    new ConfigEditor(Path.Combine(neo4jConfigFolder, Neo4jConfigFile))
                }
            };
        }

        public abstract Task Stop(CancellationToken token);

        public abstract Task Start(CancellationToken token);

        public virtual async Task Restart(CancellationToken token)
        {
            await Stop(token);
            await Start(token);
        }

        public virtual async Task Clear(CancellationToken token)
        {
            var dataPath = GetDataPath();

            await StopWhile(token, () => Directory.Delete(dataPath, true));
        }

        public virtual async Task Backup(CancellationToken token, string destinationPath, bool stopInstanceBeforeBackup = true)
        {
            var dataPath = GetDataPath();

            var action = new Action(() => fileCopy.MirrorFolders(dataPath, destinationPath));

            if (stopInstanceBeforeBackup)
            {
                await StopWhile(token, action);
            }
            else
            {
                await Task.Run(action, token);
            }
        }

        public virtual async Task Restore(CancellationToken token, string sourcePath)
        {
            var dataPath = GetDataPath();

            await StopWhile(token, () => fileCopy.MirrorFolders(sourcePath, dataPath));
        }

        public virtual void Configure(string configFile, string key, string value)
        {
            configEditors[configFile].SetValue(key, value);
        }

        public virtual Neo4jVersion Version { get; }

        public virtual Neo4jEndpoints Endpoints { get; }

        public virtual string DataPath => GetDataPath();

        protected virtual async Task StopWhile(CancellationToken token, Action action)
        {
            await Stop(token);
            await Task.Run(action, token);
            await Start(token);
        }

        protected virtual string GetDataPath()
        {
            var dataDirectory = configEditors[Neo4jConfigFile].GetValue("dbms.directories.data");
            if (string.IsNullOrEmpty(dataDirectory))
                dataDirectory = defaultDataDirectory;

            var activeDatabase = configEditors[Neo4jConfigFile].GetValue("dbms.active_database");
            if (string.IsNullOrEmpty(activeDatabase))
                activeDatabase = defaultActiveDatabase;

            return Path.Combine(neo4jHomeFolder, dataDirectory, activeDatabase);
        }
    }
}
