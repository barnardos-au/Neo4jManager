using System;
using System.Diagnostics.CodeAnalysis;

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


            instance1.Start();
            instance1.WaitForReady().Wait();
            instance2.Start();
            instance2.WaitForReady().Wait();
            instance1.Stop();
            instance2.Stop();
            instance1.Dispose();
            instance2.Dispose();
        }

        private static INeo4jInstanceProvider GetInstance1()
        {
            var options = new Neo4jOptions
            {
                HeapInitialSize = "2048m",
                HeapMaxSize = "4096m",
                Endpoints = { HttpEndpoint = new Uri("http://localhost:7474/") }
            };
            options.Parameters.Add("file.encoding", "UTF-8");

            var builder = new JavaProcessBuilderV3(JavaPath, @"C:\temp\neo4j\1\neo4j-community-3.2.0", options);

            return new JavaInstanceProviderV3(builder, options);
        }

        private static INeo4jInstanceProvider GetInstance2()
        {
            var options = new Neo4jOptions
            {
                HeapInitialSize = "2048m",
                HeapMaxSize = "4096m",
                Endpoints = { HttpEndpoint = new Uri("http://localhost:7476/") }
            };
            options.Parameters.Add("file.encoding", "UTF-8");

            var builder = new JavaProcessBuilderV3(JavaPath, @"C:\temp\neo4j\2\neo4j-community-3.2.0", options);

            return new JavaInstanceProviderV3(builder, options);
        }
    }
}