using System.Collections.Generic;
using Neo4jManager.Host.Versions;

namespace Neo4jManager.Host.Deployments
{
    public class DeploymentRequest
    {
        public string Id { get; set; }
        public string Version { get; set; }

        public IEnumerable<Version> Versions { get; set; }
    }
}
