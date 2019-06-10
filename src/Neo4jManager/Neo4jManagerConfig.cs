using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Neo4jManager
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Neo4jManagerConfig : INeo4jManagerConfig
    {
        public string Neo4jBasePath { get; set; }
        public string DeploymentsBasePath => string.IsNullOrEmpty(Neo4jBasePath) ? null : Path.Combine(Neo4jBasePath, "deployments");

        public long StartHttpPort { get; set; }
        public long StartHttpsPort { get; set; }
        public long StartBoltPort { get; set; }
    }
}
