using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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

            var neo4jV3 = new Neo4jVersion
            {
                Architecture = Neo4jArchitecture.V3,
                DownloadUrl = "https://neo4j.com/artifact.php?name=neo4j-community-3.5.3-windows.zip",
                Version = "3.5.3",
                ZipFileName = "neo4j-community-3.5.3-windows.zip"
            };

            var config = new Neo4jManagerConfig
            {
                Neo4jBasePath = @"c:\Neo4jManager",
                StartBoltPort = 7687,
                StartHttpPort = 7401
            };

            var instanceFactory = new Neo4jInstanceFactory(new FileCopy());

            using (var pool = new Neo4jDeploymentsPool(config, instanceFactory))
            {
                pool.DeleteAll();

                pool.Create(neo4jV3, "1");
                pool.Create(neo4jV3, "2");
                pool.Create(neo4jV3, "3");
                pool.Create(neo4jV3, "4");

                var task1 = Process(pool.Single(p => p.Key == "1"), ct);
                var task2 = Process(pool.Single(p => p.Key == "2"), ct);
                var task3 = Process(pool.Single(p => p.Key == "3"), ct);
                var task4 = Process(pool.Single(p => p.Key == "4"), ct);

                Task.WhenAll(task1, task2, task3, task4).Wait(ct);

                pool.DeleteAll();
            }
        }

        private static async Task Process(KeyValuePair<string, INeo4jInstance> kvp, CancellationToken token)
        {
            var instance = kvp.Value;

            await instance.Start(token);
            await instance.Backup(token, $@"C:\temp\backup\{kvp.Key}");
            await instance.Restore(token, $@"C:\temp\backup\{kvp.Key}");
            await instance.Clear(token);
        }
    }
}