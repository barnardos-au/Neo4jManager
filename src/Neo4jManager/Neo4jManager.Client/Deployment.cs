namespace Neo4jManager.Client
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
