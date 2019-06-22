using System.Threading;
using System.Threading.Tasks;

namespace Neo4jManager.Host
{
    internal interface IWatchdogService
    {
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
    }
}
