using System.Collections.Generic;
using ServiceStack;

namespace Neo4jManager.ServiceModel
{
    public class VersionsResponse : IHasResponseStatus
    {
        public IEnumerable<Version> Versions { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    }
}
