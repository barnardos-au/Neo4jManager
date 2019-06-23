using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Topshelf;

namespace Neo4jManager.Host
{
    // ReSharper disable once InconsistentNaming
    public class Neo4jManagerService : ServiceControl
    {
        private IWebHost webHost;
        
        public bool Start(HostControl hostControl)
        {
            webHost = CreateWebHostBuilder();
            webHost.RunAsync();
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            webHost.StopAsync().Wait();
            webHost.Dispose();
            return true;
        }
        
        private static IWebHost CreateWebHostBuilder()
        {
            return new WebHostBuilder()
                .UseKestrel()
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddEventLog();
                })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseUrls(Environment.GetEnvironmentVariable("Neo4jManager.Url") ?? "http://localhost:7400/")
                .Build();
        }
    }
}