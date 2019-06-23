using ServiceStack;

namespace Neo4jManager.ServiceModel
{
    [Route("/versions", "GET")]
    public class VersionsRequest : IReturn<VersionsResponse>
    {
    }
}
