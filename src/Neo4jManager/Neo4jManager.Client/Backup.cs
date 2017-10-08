namespace Neo4jManager.Client
{
    public class Backup
    {
        public string DestinationPath { get; set; }
        public bool StopInstanceBeforeBackup { get; set; } = true;
    }
}
