using ServiceStack;

namespace Neo4jManager.ServiceModel.Deployments
{
    [Route("/versions")]
    public class VersionsRequest : IReturn<VersionsResponse>
    {
    }
}
