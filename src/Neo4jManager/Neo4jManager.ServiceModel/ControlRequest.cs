using ServiceStack;

namespace Neo4jManager.ServiceModel
{
    [Route("/deployment/{Id}/{Operation}")]
    public class ControlRequest : IReturn<DeploymentResponse>
    {
        public string Id { get; set; }
        public Operation Operation { get; set; }

        // Backup
        public string DestinationPath { get; set; }
        public bool StopInstanceBeforeBackup { get; set; } = true;

        // Restore
        public string SourcePath { get; set; }
        
        // Configure
        public Setting Setting { get; set; }
    }
}
