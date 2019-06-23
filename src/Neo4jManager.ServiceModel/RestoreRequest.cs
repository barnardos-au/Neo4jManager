using ServiceStack;

namespace Neo4jManager.ServiceModel
{
    [Route("/deployment/{Id}/Restore", "POST")]
    public class RestoreRequest : IReturn<DeploymentResponse>
    {
        public string Id { get; set; }
        
        public string RestoreUrl { get; set; }
    }
}
