using System.Collections.Generic;
using System.IO;
using Neo4jManager.ServiceModel.Versions;
using ServiceStack;

namespace Neo4jManager.ServiceInterface
{
    // ReSharper disable once InconsistentNaming
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
