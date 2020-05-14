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
        Task Backup(CancellationToken token);
        Task Restore(CancellationToken token, string sourcePathOrUrl);
        void Configure(string configFile, string key, string value);
        void InstallPlugin(string sourcePathOrUrl);

        INeo4jDeployment Deployment { get; }
        Status Status { get; }
        short Offset { get; }
    }
}