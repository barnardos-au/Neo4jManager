using ServiceStack;

namespace Neo4jManager.ServiceModel.Deployments
{
    [Route("/deployment/{Id}/control/{Operation}")]
    public class ControlRequest : IReturn<DeploymentResponse>
    {
        public string Id { get; set; }
        public Operation Operation { get; set; }
    }
}
