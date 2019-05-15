using System.Collections.Generic;

namespace Neo4jManager.ServiceModel.Deployments
{
    public class DeploymentsResponse
    {
        public IEnumerable<Deployment> Deployments { get; set; }
    }
}
