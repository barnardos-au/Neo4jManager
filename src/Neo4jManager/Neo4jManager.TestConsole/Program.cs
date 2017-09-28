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

            var neo4Jversion = Neo4jVersions.GetVersions()
                .Single(p => p.Version == "3.2.3");

            var config = new Neo4jManagerConfig
            {
                Neo4jBasePath = @"c:\Neo4jManager",
                StartBoltPort = 7687,
                StartHttpPort = 7401
            };

            var instanceFactory = new Neo4jInstanceFactory(config, new FileCopy());

            using (var pool = new Neo4jInstancePool(config, instanceFactory))
            {
                pool.Create(neo4Jversion, "1");
                pool.Create(neo4Jversion, "2");
                pool.Create(neo4Jversion, "3");
                pool.Create(neo4Jversion, "4");

                var task1 = Process(pool.Instances.Single(p => p.Key == "1"), ct);
                var task2 = Process(pool.Instances.Single(p => p.Key == "2"), ct);
                var task3 = Process(pool.Instances.Single(p => p.Key == "3"), ct);
                var task4 = Process(pool.Instances.Single(p => p.Key == "4"), ct);

                Task.WhenAll(task1, task2, task3, task4).Wait(ct);
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