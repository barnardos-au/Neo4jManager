using System;
using System.Diagnostics.CodeAnalysis;

namespace Neo4jManager
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public interface INeo4jInstanceProvider : IDisposable
    {
        void Start();
        void Stop();

        Neo4jEndpoints Endpoints { get; }
    }
}