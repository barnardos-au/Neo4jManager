using System;

namespace Neo4jManager.ServiceModel
{
    public class Deployment
    {
        public string Id { get; set; }
        public string DataPath { get; set; }
        public Endpoints Endpoints { get; set; }
        public Version Version { get; set; }
        public string Status { get; set; }
        public DateTime? ExpiresOn { get; set; }
    }
}
