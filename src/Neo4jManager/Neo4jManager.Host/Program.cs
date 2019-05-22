using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Linq;
using System.ServiceProcess;

namespace Neo4jManager.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Contains("-console"))
            {
                CreateWebHost().Run();
            }
            else
            {
                using (var service = new Neo4jManagerService())
                {
                    ServiceBase.Run(service);
                }
            }
        }

        public static IWebHost CreateWebHost()
        {
            return new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseUrls(Environment.GetEnvironmentVariable("Neo4jManager.Url") ?? "http://localhost:7400/")
                .Build();
        }
    }
}
