using System.Diagnostics.CodeAnalysis;

namespace Neo4jManager
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public interface INeo4jInstanceFactory
    {
        INeo4jInstance Create(string neo4jFolder, Neo4jVersion neo4jVersion, Neo4jEndpoints endpoints);
    }
}