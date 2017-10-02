using Nancy;

namespace Neo4jManager.Host.Deployments
{
    public class DeploymentsModule : NancyModule
    {
        public DeploymentsModule() : base("/deployments")
        {
            Get("/", _ => Neo4jVersions.GetVersions().AsVersionInfos());
        }
    }
}
