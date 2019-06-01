using System;

namespace Neo4jManager
{
    public class Neo4jDeployment : INeo4jDeployment
    {
        public Neo4jVersion Version { get; set; }
        public Neo4jEndpoints Endpoints { get; set; }
        public string DataPath { get; set; }
        public DateTime? ExpiresOn { get; set; }
    }
}