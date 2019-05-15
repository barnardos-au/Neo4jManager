using Neo4jManager.ServiceModel.Versions;

namespace Neo4jManager.ServiceModel.Deployments
{
    public class Deployment
    {
        public string Id { get; set; }
        public string DataPath { get; set; }
        public Endpoints Endpoints { get; set; }
        public Version Version { get; set; }
        public string Status { get; set; }
    }
}
