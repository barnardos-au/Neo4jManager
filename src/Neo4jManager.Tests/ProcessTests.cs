using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Medallion.Shell;
using NUnit.Framework;
using Command = ServiceStack.Command;

namespace Neo4jManager.Tests
{
    [TestFixture]
    public class ProcessTests
    {
        private const string quotes = "\"";
        private const string neo4jFolder = @"c:\neo4j\neo4j-community-3.5.3-windows\neo4j-community-3.5.3";
        private const string javaToolsPath = @"C:\Program Files\Zulu\zulu\lib\tools.jar";
        private const string JavaExe = @"C:\Program Files\Zulu\zulu\jre\bin\java.exe";

        private Dictionary<string, ConfigEditor> configEditors;

        
        [Test]
        public async Task ShouldShutdownCorrectly()
        {
            var neo4JConfigFolder = Path.Combine(neo4jFolder, "conf");

            configEditors = new Dictionary<string, ConfigEditor>
            {
                {
                    Neo4jInstanceFactory.Neo4jConfigFile,
                    new ConfigEditor(Path.Combine(neo4JConfigFolder, Neo4jInstanceFactory.Neo4jConfigFile))
                }
            };

//            var command = Command.Run(JavaExe, GetRunArgs());
//            await command.TrySignalAsync(CommandSignal.ControlC);
            

            Process cmd = new Process
            {
                StartInfo =
                {
                    FileName = JavaExe,
                    Arguments = GetRunArgs(),
//                    RedirectStandardInput = true,
//                    RedirectStandardOutput = true,
                    //CreateNoWindow = true,
                    UseShellExecute = true,
                    //WorkingDirectory = @"C:\neo4j\neo4j-community-3.5.3-windows\neo4j-community-3.5.3"
                }
            };
            
            cmd.Start();

            var deployment = new Neo4jDeployment
            {
                Endpoints = new Neo4jEndpoints
                {
                    HttpEndpoint = new Uri("http://localhost:7474/")
                }
            };
            await deployment.WaitForReady(CancellationToken.None);
            
            if (Medallion.Shell.Command.TryAttachToProcess(cmd.Id, out var command))
            {
                await command.TrySignalAsync(CommandSignal.ControlC);
            }

            //cmd.StandardInput.WriteLine("echo Oscar");
            cmd.WaitForExit();

            var dumpProcess = new Process
            {
                StartInfo =
                {
                    FileName = JavaExe,
                    Arguments = GetDumpCmdArgs(),
                    CreateNoWindow = true,
                    WorkingDirectory = @"C:\neo4j\neo4j-community-3.5.3-windows\neo4j-community-3.5.3"
                }
            };
            
            dumpProcess.Start();
            dumpProcess.WaitForExit();
        }

        private string GetRunArgs()
        {
            var builder = new StringBuilder();

            builder
                .Append(" -cp ")
                .Append(quotes)
                .Append($"{neo4jFolder}/lib/*;{neo4jFolder}/plugins/*")
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
                .Append($@"{neo4jFolder}\conf")
                .Append(quotes)
                .Append(" --home-dir=")
                .Append(quotes)
                .Append(neo4jFolder)
                .Append(quotes);

            return builder.ToString();

        }
        
        private string GetDumpCmdArgs()
        {
            var builder = new StringBuilder();
            
            builder
                .Append(" -XX:+UseParallelGC")
                .Append(" -classpath ")
                .Append(quotes)
                .Append($";{neo4jFolder}/lib/*;{neo4jFolder}/bin/*;{javaToolsPath}")
                .Append(quotes)
                .Append(" -Dbasedir=")
                .Append(quotes)
                .Append(neo4jFolder)
                .Append(quotes)
                .Append(" -Dfile.encoding=UTF-8")
                .Append(" org.neo4j.commandline.admin.AdminTool dump")
                .Append(" --database=graph.db")
                .Append(" --to=")
                .Append(quotes)
                .Append(@"c:\temp\backup.dump")
                .Append(quotes);

            return builder.ToString();
        }
        
        
    }
}