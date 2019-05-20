using System.Collections.Generic;
using Neo4jManager.ServiceModel.Versions;

namespace Neo4jManager.ServiceModel.Deployments
{
    public class VersionsResponse
    {
        public IEnumerable<Version> Versions { get; set; }
    }
}
