using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Neo4jManager
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class Neo4jVersions
    {
        public static IEnumerable<Neo4jVersion> GetVersions()
        {
            return new List<Neo4jVersion>
            {
                new Neo4jVersion
                {
                    Version = "2.3.2",
                    DownloadUrl = "https://neo4j.com/artifact.php?name=neo4j-community-2.3.2-windows.zip",
                    ZipFileName = "neo4j-community-2.3.2-windows.zip",
                    Architecture = Neo4jArchitecture.V2
                },
                new Neo4jVersion
                {
                    Version = "3.2.3",
                    DownloadUrl = "https://neo4j.com/artifact.php?name=neo4j-community-3.2.3-windows.zip",
                    ZipFileName = "neo4j-community-3.2.3-windows.zip",
                    Architecture = Neo4jArchitecture.V3
                },
                new Neo4jVersion
                {
                    Version = "3.2.5",
                    DownloadUrl = "https://neo4j.com/artifact.php?name=neo4j-community-3.2.5-windows.zip",
                    ZipFileName = "neo4j-community-3.2.5-windows.zip",
                    Architecture = Neo4jArchitecture.V3
                },
                new Neo4jVersion
                {
                    Version = "3.3.0b2",
                    DownloadUrl = "https://neo4j.com/artifact.php?name=neo4j-community-3.3.0-beta02-windows.zip",
                    ZipFileName = "neo4j-community-3.3.0-beta02-windows.zip",
                    Architecture = Neo4jArchitecture.V3
                },
            };
        }
    }
}
