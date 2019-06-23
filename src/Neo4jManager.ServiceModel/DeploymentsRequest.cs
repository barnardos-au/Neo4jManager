using ServiceStack;

namespace Neo4jManager.ServiceModel
{
    [Route("/deployments", "GET,DELETE")]
    public class DeploymentsRequest : IReturn<DeploymentsResponse>
    {
        public bool Permanent { get; set; } 
    }
}
