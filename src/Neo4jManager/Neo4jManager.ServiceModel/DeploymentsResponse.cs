using System.Collections.Generic;
using ServiceStack;

namespace Neo4jManager.ServiceModel
{
    public class DeploymentsResponse : IHasResponseStatus
    {
        public IEnumerable<Deployment> Deployments { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    }
}
