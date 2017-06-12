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
        void Configure(string key, string value);
        Task Clear();
        Task Backup(string destinationPath, bool stopInstanceBeforeBackup = true);
        Task Restore(string sourcePath);
        
        Neo4jEndpoints Endpoints { get; }
    }
}