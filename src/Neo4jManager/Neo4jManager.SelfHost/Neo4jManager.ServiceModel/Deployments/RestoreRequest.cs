using ServiceStack;

namespace Neo4jManager.ServiceModel.Deployments
{
    [Route("/deployment/{Id}/restore")]
    public class RestoreRequest : IReturn<DeploymentResponse>
    {
        public string Id { get; set; }
        public string SourcePath { get; set; }
    }
}
