using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Neo4jManager
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public interface INeo4jInstancePool : IDisposable
    {
        INeo4jInstance Create(Neo4jVersion neo4jVersion, string id);
        void Reset();
        Dictionary<string, INeo4jInstance> Instances { get; }
    }
}