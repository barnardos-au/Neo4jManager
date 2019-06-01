using System;

namespace Neo4jManager
{
    public interface INeo4jDeployment
    {
        Neo4jVersion Version { get; }
        Neo4jEndpoints Endpoints { get; }
        string DataPath { get; }
        DateTime? ExpiresOn { get; }
    }
}