namespace Neo4jManager.Host.Deployments
{
    public class BackupRequest
    {
        public string DestinationPath { get; set; }
        public bool StopInstanceBeforeBackup { get; set; } = true;
    }
}
