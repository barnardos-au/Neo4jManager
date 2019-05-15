using ServiceStack;

namespace Neo4jManager.ServiceModel.Deployments
{
    [Route("/deployment/{Id}/backup")]
    public class BackupRequest : IReturn<DeploymentResponse>
    {
        public string Id { get; set; }
        public string DestinationPath { get; set; }
        public bool StopInstanceBeforeBackup { get; set; } = true;
    }
}
