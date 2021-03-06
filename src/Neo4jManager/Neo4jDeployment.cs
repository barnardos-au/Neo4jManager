using System;

namespace Neo4jManager
{
    // ReSharper disable once InconsistentNaming
    public class Neo4jDeployment : INeo4jDeployment
    {
        public Neo4jVersion Version { get; set; }
        public Neo4jEndpoints Endpoints { get; set; }
        public string DataPath { get; set; }
        public DateTime? ExpiresOn { get; set; }
        public string BackupPath { get; set; }
        public string LastBackupFile { get; set; }
    }
}