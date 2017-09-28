using System.Diagnostics.CodeAnalysis;

namespace Neo4jManager
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public interface INeo4jInstanceFactory
    {
        INeo4jInstance Create(Neo4jVersion neo4jVersion, string targetDeploymentPath, int portOffset);
    }
}