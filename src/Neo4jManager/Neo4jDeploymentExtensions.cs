using System;

namespace Neo4jManager
{
    // ReSharper disable once InconsistentNaming
    public static class Neo4jDeploymentExtensions
    {
        public static bool IsExpired(this INeo4jDeployment neo4JDeployment)
        {
            if (neo4JDeployment.ExpiresOn == null) return false;

            return DateTime.UtcNow > neo4JDeployment.ExpiresOn;
        }
    }
}