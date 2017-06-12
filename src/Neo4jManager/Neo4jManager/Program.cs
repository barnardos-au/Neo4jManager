using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Neo4jManager
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal class Program
    {
        private const string JavaPath = @"C:\Program Files\Java\jre1.8.0_131\bin\java.exe";

        private static void Main()
        {
            var instance1 = GetInstance1();
            var instance2 = GetInstance2();
            var task1 = Process(instance1);
            var task2 = Process(instance2);

            Task.WhenAll(task1, task2).Wait();

            instance1.Dispose();
            instance2.Dispose();
        }

        private static INeo4jInstanceProvider GetInstance1()
        {
            var endpoints = new Neo4jEndpoints
            {
                BoltEndpoint = new Uri("bolt://localhost:7687"),
                HttpEndpoint = new Uri("http://localhost:7474")
            };
            //return new JavaInstanceProviderV3(JavaPath, @"C:\temp\neo4j\1\neo4j-community-3.2.0", endpoints, new FileCopy());
            //return new PowerShellInstanceProvider( @"C:\temp\neo4j\1\neo4j-community-3.2.0", endpoints, new FileCopy());
            return new ServiceInstanceProvider(@"C:\temp\neo4j\1\neo4j-community-3.2.0", endpoints, new FileCopy());
        }

        private static INeo4jInstanceProvider GetInstance2()
        {
            var endpoints = new Neo4jEndpoints
            {
                BoltEndpoint = new Uri("bolt://localhost:7688"),
                HttpEndpoint = new Uri("http://localhost:7476")
            };

            return new ServiceInstanceProvider(@"C:\temp\neo4j\2\neo4j-community-3.2.0", endpoints, new FileCopy());
        }

        private static async Task Process(INeo4jInstanceProvider instance)
        {
            await instance.Start();
            //await instance.Backup(@"C:\temp\backup");
            //await instance.Restore(@"C:\temp\backup");
        }
    }
}