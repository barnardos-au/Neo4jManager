using System.Collections.Generic;

namespace Neo4jManager.ServiceModel
{
    public class DeploymentsResponse
    {
        public IEnumerable<Deployment> Deployments { get; set; }
    }
}
