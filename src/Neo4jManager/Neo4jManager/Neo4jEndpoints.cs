using System;
using System.Diagnostics.CodeAnalysis;

namespace Neo4jManager
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Neo4jEndpoints
    {
        public Uri HttpEndpoint { get; set; }
        public Uri HttpsEndpoint { get; set; }
        public Uri BoltEndpoint { get; set; }
    }
}
