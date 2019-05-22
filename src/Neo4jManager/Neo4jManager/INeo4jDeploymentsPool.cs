using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Neo4jManager
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public interface INeo4jDeploymentsPool : IDictionary<string, INeo4jInstance>, IDisposable
    {
        INeo4jInstance Create(Neo4jVersion neo4jVersion, string id, string[] pluginsUrl);
        void Delete(string id);
        void DeleteAll();
    }
}