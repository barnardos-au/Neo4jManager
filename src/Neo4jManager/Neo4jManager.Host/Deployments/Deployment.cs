using Neo4jManager.Host.Versions;

namespace Neo4jManager.Host.Deployments
{
    public class Deployment
    {
        public string Id { get; set; }
        public string DataPath { get; set; }
        public Endpoints Endpoints { get; set; }
        public Version Version { get; set; }
    }
}
