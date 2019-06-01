using ServiceStack;

namespace Neo4jManager.ServiceModel
{
    [Route("/deployment/{Id}", "GET,DELETE")]
    public class DeploymentRequest : IReturn<DeploymentResponse>
    {
        public string Id { get; set; }
    }
}
