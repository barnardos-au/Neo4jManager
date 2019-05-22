using ServiceStack;

namespace Neo4jManager.ServiceModel
{
    [Route("/versions")]
    public class VersionsRequest : IReturn<VersionsResponse>
    {
    }
}
