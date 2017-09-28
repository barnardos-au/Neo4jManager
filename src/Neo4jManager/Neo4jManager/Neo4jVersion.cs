using System.Diagnostics.CodeAnalysis;

namespace Neo4jManager
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Neo4jVersion
    {
        public string DownloadUrl { get; set; }
        public string ZipFileName { get; set; }
        public string Version { get; set; }
        public Neo4jArchitecture Architecture { get; set; }
    }
}
