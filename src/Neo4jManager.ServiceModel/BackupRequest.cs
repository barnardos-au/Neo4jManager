using System.IO;
using ServiceStack;

namespace Neo4jManager.ServiceModel
{
    [Route("/deployment/{Id}/Backup", "POST")]
    public class BackupRequest : IReturn<Stream>
    {
        public string Id { get; set; }
        
        public bool ObtainLastBackup { get; set; }
    }
}
