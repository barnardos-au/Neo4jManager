using ServiceStack;

namespace Neo4jManager.ServiceModel.Deployments
{
    [Route("/")]
    [Route("/deployments")]
    [Route("/deployments/all", "DELETE")]
    public class DeploymentsRequest : IReturn<DeploymentsResponse>
    {
    }
}
