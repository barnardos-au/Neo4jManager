using System;

namespace Neo4jManager
{
    // ReSharper disable once InconsistentNaming
    public class Neo4jDeploymentRequest
    {
        public Neo4jVersion Version { get; set; }
        public Neo4jEndpoints Endpoints { get; set; }
        public TimeSpan? LeasePeriod { get; set; }
        public string Neo4jFolder { get; set; }
        public short Offset { get; set; }
    }
}