using System.Diagnostics.CodeAnalysis;

namespace Neo4jManager
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public interface INeo4jManagerConfig
    {
        string Neo4jBasePath { get; set; }
        long StartHttpPort { get; set; }
        long StartHttpsPort { get; set; }
        long StartBoltPort { get; set; }
    }
}