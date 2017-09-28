using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Neo4jManager
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public interface INeo4jInstance : IDisposable
    {
        Task Start(CancellationToken token);
        Task Stop(CancellationToken token);
        Task Restart(CancellationToken token);
        Task Clear(CancellationToken token);
        Task Backup(CancellationToken token, string destinationPath, bool stopInstanceBeforeBackup = true);
        Task Restore(CancellationToken token, string sourcePath);
        void Configure(string key, string value);

        Neo4jEndpoints Endpoints { get; }
        string DataPath { get; }
    }
}