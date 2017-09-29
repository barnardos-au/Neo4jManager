using Funq;
using ServiceStack;

namespace Neo4jManager.Host
{
    public class AppHost : AppHostBase
    {
        public AppHost() : base("Hello Service", typeof(HelloService).GetAssembly()) { }

        public override void Configure(Container container)
        {
        }
    }
}
