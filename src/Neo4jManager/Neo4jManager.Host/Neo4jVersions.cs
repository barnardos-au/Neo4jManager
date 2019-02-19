using System.Collections.Generic;
using System.IO;
using ServiceStack;
using Version = Neo4jManager.Host.Versions.Version;

namespace Neo4jManager.Host
{
    public class Neo4jVersionRepository : INeo4jVersionRepository
    {
        private readonly string versionsFilePath;

        public Neo4jVersionRepository(
            string versionsFilePath)
        {
            this.versionsFilePath = versionsFilePath;
        }

        public IEnumerable<Version> GetVersions()
        {
            var json = File.ReadAllText(versionsFilePath);
            return json.FromJson<IEnumerable<Version>>();
        }
    }
}
