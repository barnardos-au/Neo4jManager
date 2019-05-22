using ServiceStack;

namespace Neo4jManager.ServiceModel
{
    public class DeploymentResponse : IHasResponseStatus
    {
        public Deployment Deployment { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    }
}
