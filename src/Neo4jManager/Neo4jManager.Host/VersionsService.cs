using System.Linq;
using ServiceStack;

namespace Neo4jManager.Host
{
    public class VersionsService : Service
    {
        public object Any(Versions request)
        {
            return new VersionsResponse
            {
                Versions = Neo4jVersions.GetVersions().ToList().Select(p => new VersionInfo
                {
                    Version = p.Version,
                    Architecture = p.Architecture.ToString()
                })
            };
        }
    }

}
