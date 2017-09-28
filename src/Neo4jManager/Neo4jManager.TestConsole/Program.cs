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
                Neo4jBasePath = @"c:\Neo4jManager\neo4j",
                StartBoltPort = 7687,
                StartHttpPort = 7401
            };

            using (var pool = new Neo4jInstancePool(new FileCopy(), config))
            {
                var instance1 = pool.Create(neo4Jversion, "1");
                var instance2 = pool.Create(neo4Jversion, "2");

                var task1 = Process(instance1, ct);
                var task2 = Process(instance2, ct);
                Task.WhenAll(task1, task2).Wait(ct);
            }
        }

        private static async Task Process(INeo4jInstance instance, CancellationToken token)
        {
            var port = instance.Endpoints.HttpEndpoint.Port;
            await instance.Start(token);
            await instance.Backup(token, $@"C:\temp\backup\{port}");
            await instance.Restore(token, $@"C:\temp\backup\{port}");
        }
    }
}