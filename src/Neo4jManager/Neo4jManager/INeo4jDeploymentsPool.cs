using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Neo4jManager
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public interface INeo4jDeploymentsPool : IDisposable
    {
        INeo4jInstance Create(Neo4jVersion neo4jVersion, string id);
        void Delete(string id);
        void DeleteAll();
        Dictionary<string, INeo4jInstance> Deployments { get; }
    }
}