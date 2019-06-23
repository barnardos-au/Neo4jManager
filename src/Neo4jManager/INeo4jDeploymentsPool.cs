using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Neo4jManager
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public interface INeo4jDeploymentsPool : IDictionary<string, INeo4jInstance>, IDisposable
    {
        string Create(Neo4jDeploymentRequest request);
        void Delete(string id, bool permanent);
        void DeleteAll(bool permanent);
    }
}