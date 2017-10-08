using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;

namespace Neo4jManager.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var runAsService = args.Any() && args[0].Contains("--service");

            var pathToContentRoot = Directory.GetCurrentDirectory();

            if (runAsService)
            {
                var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
                pathToContentRoot = Path.GetDirectoryName(pathToExe);
            }

            var host = new WebHostBuilder()
                .UseKestrel(options => options.Listen(IPAddress.Loopback, 7400))
                .ConfigureServices(services => services.AddAutofac())
                .UseContentRoot(pathToContentRoot)
                .UseStartup<Startup>()
                .Build();

            if (runAsService)
            {
                host.RunAsService();
            }
            else
            {
                host.Run();
            }
        }
    }
}
