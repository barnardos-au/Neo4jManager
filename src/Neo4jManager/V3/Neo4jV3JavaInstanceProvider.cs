using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Medallion.Shell;

namespace Neo4jManager.V3
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Neo4jV3JavaInstanceProvider : INeo4jInstance
    {
        private const string quotes = "\"";
        private const string defaultDataDirectory = "data/databases";
        private const string defaultActiveDatabase = "graph.db";

        private const int defaultWaitForKill = 10000;

        private readonly IJavaResolver javaResolver;
        private readonly IFileCopy fileCopy;
        private readonly Neo4jDeploymentRequest request;
        private readonly Dictionary<string, ConfigEditor> configEditors;
        private readonly Neo4jDeployment deployment;

        private Process process;

        public Neo4jV3JavaInstanceProvider(IJavaResolver javaResolver, IFileCopy fileCopy, Neo4jDeploymentRequest request)
        {
            this.javaResolver = javaResolver;
            this.fileCopy = fileCopy;
            this.request = request;

            var neo4JConfigFolder = Path.Combine(request.Neo4jFolder, "conf");

            configEditors = new Dictionary<string, ConfigEditor>
            {
                {
                    Neo4jInstanceFactory.Neo4jConfigFile,
                    new ConfigEditor(Path.Combine(neo4JConfigFolder, Neo4jInstanceFactory.Neo4jConfigFile))
                }
            };

            deployment = new Neo4jDeployment
            {
                DataPath = GetDataPath(),
                Endpoints = request.Endpoints,
                Version = request.Version,
                ExpiresOn = request.LeasePeriod == null
                    ? (DateTime?) null
                    : DateTime.UtcNow.Add(request.LeasePeriod.Value)
            };
        }

        public async Task Start(CancellationToken token)
        {
            if (process == null)
            {
                process = GetProcess(GetNeo4jStartArguments());
                process.Start();
                await deployment.WaitForReady(token);

                Status = Status.Started;
                return;
            }

            if (!process.HasExited) return;
            
            process.Start();
            await deployment.WaitForReady(token);
            Status = Status.Started;
        }

        public async Task Stop(CancellationToken token)
        {
            Status = Status.Stopping;

            if (process == null || process.HasExited) return;

            if (Command.TryAttachToProcess(process.Id, out var command))
            {
                await command.TrySignalAsync(CommandSignal.ControlC);
            }

            process.WaitForExit(defaultWaitForKill);

            Status = Status.Stopped;
        }
        
        public async Task Restart(CancellationToken token)
        {
            await Stop(token);
            await Start(token);
        }
        
        public void Configure(string configFile, string key, string value)
        {
            configEditors[configFile].SetValue(key, value);
        }

        public void DownloadPlugin(string pluginUrl)
        {
            var pluginsFolder = Path.Combine(request.Neo4jFolder, "plugins");
            Helper.DownloadFile(pluginUrl, pluginsFolder);
        }

        public async Task Clear(CancellationToken token)
        {
            var dataPath = GetDataPath();

            await StopWhile(token, () => Directory.Delete(dataPath, true));
        }

        public async Task Backup(CancellationToken token, string destinationPath, bool stopInstanceBeforeBackup = true)
        {
            var dataPath = GetDataPath();

            if (stopInstanceBeforeBackup)
            {
                await StopWhile(token, () =>
                {
                    var arguments = GetDumpArguments(destinationPath);
                    using (var dumpProcess = GetProcess(arguments))
                    {
                        dumpProcess.StartInfo.WorkingDirectory = request.Neo4jFolder;
            
                        dumpProcess.Start();
                        dumpProcess.WaitForExit();
                    }
                });
            }
            else
            {
                await Task.Run(
                    () => fileCopy.MirrorFolders(dataPath, destinationPath), token);
            }
        }

        public async Task Restore(CancellationToken token, string sourcePath)
        {
            var dataPath = GetDataPath();

            await StopWhile(token, () => fileCopy.MirrorFolders(sourcePath, dataPath));
        }

        public INeo4jDeployment Deployment => deployment;

        public Status Status { get; private set; } = Status.Stopped;

        
        public void Dispose()
        {
            AsyncHelper.RunSync(() => Stop(CancellationToken.None));

            process?.Dispose();
        }

        private async Task StopWhile(CancellationToken token, Action action)
        {
            await Stop(token);
            await Task.Run(action, token);
            await Start(token);
        }

        private Process GetProcess(string arguments)
        {
            return new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = javaResolver.GetJavaPath(),
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
            };
        }

        private string GetDumpArguments(string destinationPath)
        {
            var javaToolsPath = javaResolver.GetToolsPath();
            var builder = new StringBuilder();

            builder
                .Append(" -XX:+UseParallelGC")
                .Append(" -classpath ")
                .Append(quotes)
                .Append($";{request.Neo4jFolder}/lib/*;{request.Neo4jFolder}/bin/*;{javaToolsPath}")
                .Append(quotes)
                .Append(" -Dbasedir=")
                .Append(quotes)
                .Append(request.Neo4jFolder)
                .Append(quotes)
                .Append(" org.neo4j.commandline.admin.AdminTool dump")
                .Append(" --database=graph.db")
                .Append(" --to=")
                .Append(quotes)
                .Append(destinationPath)
                .Append(quotes);

            return builder.ToString();
        }

        private string GetNeo4jStartArguments()
        {
            var builder = new StringBuilder();

            builder
                .Append(" -cp ")
                .Append(quotes)
                .Append($"{request.Neo4jFolder}/lib/*;{request.Neo4jFolder}/plugins/*")
                .Append(quotes);

            builder.Append(" -server");

            builder.Append(" -Dlog4j.configuration=file:conf/log4j.properties");
            builder.Append(" -Dneo4j.ext.udc.source=zip-powershell");
            builder.Append(" -Dorg.neo4j.cluster.logdirectory=data/log");

            var jvmAdditionalParams = configEditors[Neo4jInstanceFactory.Neo4jConfigFile]
                .FindValues("dbms.jvm.additional")
                .Select(p => p.Value);

            foreach (var param in jvmAdditionalParams)
            {
                builder.Append($" {param}");
            }

            var heapInitialSize = configEditors[Neo4jInstanceFactory.Neo4jConfigFile].GetValue("dbms.memory.heap.initial_size");
            if (!string.IsNullOrEmpty(heapInitialSize))
            {
                builder.Append($" -Xms{heapInitialSize}");
            }
            var heapMaxSize = configEditors[Neo4jInstanceFactory.Neo4jConfigFile].GetValue("dbms.memory.heap.max_size");
            if (!string.IsNullOrEmpty(heapMaxSize))
            {
                builder.Append($" -Xmx{heapMaxSize}");
            }

            builder
                .Append(" org.neo4j.server.CommunityEntryPoint")
                .Append(" --config-dir=")
                .Append(quotes)
                .Append($@"{request.Neo4jFolder}\conf")
                .Append(quotes)
                .Append(" --home-dir=")
                .Append(quotes)
                .Append(request.Neo4jFolder)
                .Append(quotes);

            return builder.ToString();
        }
        
        private string GetDataPath()
        {
            var dataDirectory = configEditors[Neo4jInstanceFactory.Neo4jConfigFile].GetValue("dbms.directories.data");
            if (string.IsNullOrEmpty(dataDirectory))
                dataDirectory = defaultDataDirectory;

            var activeDatabase = configEditors[Neo4jInstanceFactory.Neo4jConfigFile].GetValue("dbms.active_database");
            if (string.IsNullOrEmpty(activeDatabase))
                activeDatabase = defaultActiveDatabase;

            return Path.Combine(request.Neo4jFolder, dataDirectory, activeDatabase);
        }
    }
    
}
