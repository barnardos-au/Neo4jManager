using System.Collections.Generic;
using ServiceStack;

namespace Neo4jManager.Host
{
    [Route("/versions")]
    public class Versions : IReturn<VersionsResponse>
    {
    }

    public class VersionsResponse
    {
        public IEnumerable<VersionInfo> Versions { get; set; }
    }
}
