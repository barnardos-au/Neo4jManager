using System.Collections.Generic;
using Neo4jManager.Host.Versions;

namespace Neo4jManager.Host
{
    // ReSharper disable once InconsistentNaming
    public interface INeo4jVersionRepository
    {
        IEnumerable<Version> GetVersions();
    }
}