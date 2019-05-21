using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;

namespace Neo4jManager.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseUrls(Environment.GetEnvironmentVariable("Neo4jManager.Url") ?? "http://localhost:7400/")
                .Build();

            host.Run();
        }
    }
}
