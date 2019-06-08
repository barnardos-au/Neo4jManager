using ServiceStack;

namespace Neo4jManager.ServiceModel
{
    [Route("/deployment/{Id}/{Operation}", "POST")]
    public class ControlRequest : IReturn<DeploymentResponse>
    {
        public string Id { get; set; }
        public Operation Operation { get; set; }

        // Restore
        public string SourcePath { get; set; }
        
        // Configure
        public Setting Setting { get; set; }
    }
}
