using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Neo4jManager
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal class Program
    {
        private static void Main()
        {
            var tokenSource = new CancellationTokenSource();
            var ct = tokenSource.Token;

            DownloadNeo4j("3.2.3");
            ExtractNeo4j("3.2.3");

            CopyNeo4jDeployment("3.2.3", @"C:\temp\Neo4jManager\neo4j\1");
            var neo4J1Folder = Directory.GetDirectories(@"C:\temp\Neo4jManager\neo4j\1")[0];
            var instance1 = GetInstance1(neo4J1Folder);

            CopyNeo4jDeployment("3.2.3", @"C:\temp\Neo4jManager\neo4j\2");
            var neo4J2Folder = Directory.GetDirectories(@"C:\temp\Neo4jManager\neo4j\2")[0];
            var instance2 = GetInstance2(neo4J2Folder);

            var task1 = Process(instance1, ct);
            var task2 = Process(instance2, ct);

            Task.WhenAll(task1, task2).Wait(ct);

            instance1.Dispose();
            instance2.Dispose();
        }

        private static INeo4jInstance GetInstance1(string neo4JFolder)
        {
            var endpoints = new Neo4jEndpoints
            {
                HttpEndpoint = new Uri("http://localhost:7401"),
                BoltEndpoint = new Uri("bolt://localhost:7687")
            };

            var instance = new JavaInstanceProviderV3(FindJavaExe(), neo4JFolder, new FileCopy(), endpoints);
            //return new PowerShellInstanceProvider(neo4JFolder, new FileCopy(), endpoints);
            //return new ServiceInstanceProvider(neo4JFolder, new FileCopy(), endpoints);

            instance.Configure("dbms.security.auth_enabled", "false");
            instance.Configure("dbms.allow_format_migration", "true");
            instance.Configure("dbms.jvm.additional.1", "-Dfile.encoding=UTF-8");
            instance.Configure("dbms.directories.import", "");
            instance.Configure("dbms.connector.http.listen_address", $":{endpoints.HttpEndpoint.Port}");
            instance.Configure("dbms.connector.bolt.listen_address", $":{endpoints.BoltEndpoint.Port}");
            instance.Configure("dbms.connector.https.enabled", "false");
            //instance.Configure("dbms.connector.bolt.enabled", "false");

            return instance;
        }

        private static INeo4jInstance GetInstance2(string neo4JFolder)
        {
            var endpoints = new Neo4jEndpoints
            {
                HttpEndpoint = new Uri("http://localhost:7402"),
                BoltEndpoint = new Uri("bolt://localhost:7688")
            };

            var instance = new JavaInstanceProviderV3(FindJavaExe(), neo4JFolder, new FileCopy(), endpoints);
            //return new PowerShellInstanceProvider(neo4JFolder, new FileCopy(), endpoints);
            //return new ServiceInstanceProvider(neo4JFolder, new FileCopy(), endpoints);

            instance.Configure("dbms.security.auth_enabled", "false");
            instance.Configure("dbms.allow_format_migration", "true");
            instance.Configure("dbms.jvm.additional.1", "-Dfile.encoding=UTF-8");
            instance.Configure("dbms.directories.import", "");
            instance.Configure("dbms.connector.http.listen_address", $":{endpoints.HttpEndpoint.Port}");
            instance.Configure("dbms.connector.bolt.listen_address", $":{endpoints.BoltEndpoint.Port}");
            instance.Configure("dbms.connector.https.enabled", "false");
            //instance.Configure("dbms.connector.bolt.enabled", "false");

            return instance;
        }

        private static async Task Process(INeo4jInstance instance, CancellationToken token)
        {
            var port = instance.Endpoints.HttpEndpoint.Port;
            await instance.Start(token);
            await instance.Backup(token, $@"C:\temp\backup\{port}");
            await instance.Restore(token, $@"C:\temp\backup\{port}");
        }

        private static string FindJavaExe()
        {
            var javaRoot = @"C:\Program Files\Java";
            return Directory.GetFiles(javaRoot, "java.exe", SearchOption.AllDirectories)
                .ToList().OrderByDescending(p => p).FirstOrDefault();
        }

        private static void DownloadNeo4j(string version)
        {
            var neo4Jversion = Neo4jVersions.GetVersions()
                .Single(p => p.Version == version);

            var zipFile = Path.Combine(@"c:\Neo4jManager\neo4j", neo4Jversion.ZipFileName);

            if (File.Exists(zipFile)) return;

            var fileInfo = new FileInfo(zipFile);
            Directory.CreateDirectory(fileInfo.DirectoryName);

            using (var webClient = new WebClient())
            {
                webClient.DownloadFile(neo4Jversion.DownloadUrl, zipFile);
            }
        }

        public static void ExtractNeo4j(string version)
        {
            var neo4Jversion = Neo4jVersions.GetVersions()
                .Single(p => p.Version == version);

            var zipFile = Path.Combine(@"c:\Neo4jManager\neo4j", neo4Jversion.ZipFileName);
            var extractFolder = Path.Combine(@"c:\Neo4jManager\neo4j", Path.GetFileNameWithoutExtension(neo4Jversion.ZipFileName));

            if (Directory.Exists(extractFolder)) return;

            ZipFile.ExtractToDirectory(zipFile, extractFolder);
        }

        public static void CopyNeo4jDeployment(string version, string targetDeploymentPath)
        {
            var neo4Jversion = Neo4jVersions.GetVersions()
                .Single(p => p.Version == version);

            var extractFolder = Path.Combine(@"c:\Neo4jManager\neo4j", Path.GetFileNameWithoutExtension(neo4Jversion.ZipFileName));

            new FileCopy().MirrorFolders(extractFolder, targetDeploymentPath);
        }
    }
}