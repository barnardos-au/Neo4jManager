using System.Diagnostics.CodeAnalysis;

namespace Neo4jManager
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Neo4jManagerConfig : INeo4jManagerConfig
    {
        public string Neo4jBasePath { get; set; }

        public long StartHttpPort { get; set; }

        public long StartBoltPort { get; set; }
    }
}
