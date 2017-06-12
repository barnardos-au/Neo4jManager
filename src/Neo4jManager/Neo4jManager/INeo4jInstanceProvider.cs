using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Neo4jManager
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public interface INeo4jInstanceProvider : IDisposable
    {
        Task Start();
        Task Stop();
        Task Restart();
        Task Clear();
        Task Backup(string destinationPath, bool stopInstanceBeforeBackup = true);
        Task Restore(string sourcePath);
        void Configure(string key, string value);

        Neo4jEndpoints Endpoints { get; }
    }
}