using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Neo4jManager.V2
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public abstract class Neo4jV2ProcessBasedInstanceProvider
    {
        public const string LoggingPropertiesConfigFile = "logging.properties";
        public const string Neo4jPropertiesConfigFile = "neo4j.properties";
        public const string Neo4jServierPropertiesConfigFile = "neo4j-server.properties";
        public const string Neo4jWrapperConfigFile = "neo4j-wrapper.conf";

        protected const string quotes = "\"";

        protected const string defaultActiveDatabase = "data/graph.db";

        private readonly IFileCopy fileCopy;

        protected readonly string neo4jHomeFolder;
        protected readonly string neo4jConfigFolder;
        protected readonly Dictionary<string, ConfigEditor> configEditors;

        protected Neo4jV2ProcessBasedInstanceProvider(string neo4jHomeFolder, IFileCopy fileCopy, Neo4jEndpoints endpoints)
        {
            this.neo4jHomeFolder = neo4jHomeFolder;
            this.fileCopy = fileCopy;

            neo4jConfigFolder = Path.Combine(neo4jHomeFolder, "conf");
            Endpoints = endpoints;

            configEditors = new Dictionary<string, ConfigEditor>
            {
                {
                    LoggingPropertiesConfigFile,
                    new ConfigEditor(Path.Combine(neo4jConfigFolder, LoggingPropertiesConfigFile))
                },
                {
                    Neo4jPropertiesConfigFile,
                    new ConfigEditor(Path.Combine(neo4jConfigFolder, Neo4jPropertiesConfigFile))
                },
                {
                    Neo4jServierPropertiesConfigFile,
                    new ConfigEditor(Path.Combine(neo4jConfigFolder, Neo4jServierPropertiesConfigFile))
                },
                {
                    Neo4jWrapperConfigFile,
                    new ConfigEditor(Path.Combine(neo4jConfigFolder, Neo4jWrapperConfigFile))
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

            await StopWhile(token, () =>
            {
                Directory.Delete(dataPath, true);
            });
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
            var activeDatabase = configEditors[Neo4jServierPropertiesConfigFile].GetValue("org.neo4j.server.database.location");
            if (string.IsNullOrEmpty(activeDatabase))
                activeDatabase = defaultActiveDatabase;

            return Path.Combine(neo4jHomeFolder, activeDatabase);
        }
    }
}
