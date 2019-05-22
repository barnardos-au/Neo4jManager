using System.ServiceProcess;
using Microsoft.AspNetCore.Hosting;

namespace Neo4jManager.Host
{
    // ReSharper disable once InconsistentNaming
    public class Neo4jManagerService : ServiceBase
    {
        private IWebHost webHost;
        
        public Neo4jManagerService()
        {
            ServiceName = "Neo4jManager";
        }
 
        protected override void OnStart(string[] args)
        {
            webHost = Program.CreateWebHost();
            webHost.StartAsync();
        }
 
        protected override void OnStop()
        {
            AsyncHelper.RunSync(() => webHost.StopAsync());
        }
    }
}
