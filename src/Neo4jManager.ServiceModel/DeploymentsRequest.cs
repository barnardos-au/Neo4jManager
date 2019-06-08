using ServiceStack;

namespace Neo4jManager.ServiceModel
{
//    [Route("/")]
    [Route("/deployments", "DELETE")]
    [Route("/deployments/all", "DELETE")]
    public class DeploymentsRequest : IReturn<DeploymentsResponse>
    {
    }
}
