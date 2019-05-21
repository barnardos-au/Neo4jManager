using System.Collections.Generic;

namespace Neo4jManager.ServiceModel
{
    public class VersionsResponse
    {
        public IEnumerable<Version> Versions { get; set; }
    }
}
