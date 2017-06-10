using System;
using System.Diagnostics.CodeAnalysis;

namespace Neo4jManager
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal class Program
    {
        private const string JavaPath = @"C:\Program Files\Java\jre1.8.0_131\bin\java.exe";
        private const string Neo4jHomeFolder = @"C:\neo4j\neo4j-community-3.1.3-windows\neo4j-community-3.1.3";

        private static void Main()
        {
            var options = new Neo4jOptions
            {
                HeapInitialSize = "2048m",
                HeapMaxSize = "4096m"
            };
            options.Parameters.Add("file.encoding", "UTF-8");

            using (var instance = new JavaInstanceProvider(JavaPath, Neo4jHomeFolder, options))
            {
                instance.Start();
                Console.WriteLine("Done");
                Console.ReadLine();
            }
        }
    }
}