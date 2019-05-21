using ServiceStack;

namespace Neo4jManager.ServiceModel
{
//    [Route("/")]
    [Route("/deployments")]
    [Route("/deployments/all", "DELETE")]
    public class DeploymentsRequest : IReturn<DeploymentsResponse>
    {
    }
}
