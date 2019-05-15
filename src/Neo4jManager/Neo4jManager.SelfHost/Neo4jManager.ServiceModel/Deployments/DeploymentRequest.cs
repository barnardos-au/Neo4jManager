using ServiceStack;

namespace Neo4jManager.ServiceModel.Deployments
{
    [Route("/deployment", "POST")]
    [Route("/deployment/{Id}", "GET,DELETE")]
    public class DeploymentRequest : IReturn<DeploymentResponse>
    {
        public string Id { get; set; }
        public string Version { get; set; }
    }
}
