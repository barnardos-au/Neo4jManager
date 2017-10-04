using Neo4jManager.Host.Versions;

namespace Neo4jManager.Host.Deployments
{
    public class DeploymentInfo
    {
        public string Id { get; set; }
        public string DataPath { get; set; }
        public EndpointsInfo Endpoints { get; set; }
        public VersionInfo Version { get; set; }
    }
}
