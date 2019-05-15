using System.Collections.Generic;
using Neo4jManager.ServiceModel.Versions;

namespace Neo4jManager.ServiceInterface
{
    // ReSharper disable once InconsistentNaming
    public interface INeo4jVersionRepository
    {
        IEnumerable<Version> GetVersions();
    }
}