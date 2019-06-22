using System;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack.Configuration;
using ServiceStack.Logging;

namespace Neo4jManager.Host
{
    internal class WatchdogService : IWatchdogService, IDisposable
    {
        private readonly ILog logger;
        private readonly IAppSettings appSettings;
        private readonly INeo4jDeploymentsPool pool;
        private Timer timer;

        public WatchdogService(
            ILog logger,
            IAppSettings appSettings,
            INeo4jDeploymentsPool pool)
        {
            this.logger = logger;
            this.appSettings = appSettings;
            this.pool = pool;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var interval = appSettings.Get<TimeSpan>(AppSettingsKeys.WatchDogInterval);
            var defaultLeasePeriod = appSettings.Get<TimeSpan>(AppSettingsKeys.DefaultLeasePeriod);
            var expiryInstanceCleanup = appSettings.Get<TimeSpan>(AppSettingsKeys.ExpiryPeriod);

            var intervalMs = Convert.ToInt32(Math.Ceiling(interval.TotalMilliseconds));
            
            logger.Info($"Watchdog service is starting...");
            logger.Info($"Watchdog interval: {interval}");
            logger.Info($"Default lease period: {defaultLeasePeriod}");
            logger.Info($"Expired instances clean-up after: {expiryInstanceCleanup}");

            timer = new Timer(async state =>
            {
                await CheckForExpiration(cancellationToken);
                timer.Change(intervalMs, Timeout.Infinite);
            }, null, 0, Timeout.Infinite);

            return Task.CompletedTask;
        }

        private async Task CheckForExpiration(CancellationToken cancellationToken)
        {
            logger.Info("Watchdog checking for expired leases");
            
            foreach (var (key, value) in pool)
            {
                if (!value.Deployment.IsExpired()) continue;

                if (value.Status != Status.Deleted)
                {
                    logger.Info($"Deployment {key} expired on {DateTime.UtcNow}");
                    logger.Info($"Backup {key}");
                    await pool[key].Backup(cancellationToken);
                    logger.Info($"Deleting {key}");
                    pool.Delete(key, false);
                    
                    continue;
                }

                var expiryInstanceCleanup = appSettings.Get<TimeSpan>(AppSettingsKeys.ExpiryPeriod).TotalSeconds;

                if (DateTime.UtcNow > value.Deployment.ExpiresOn.GetValueOrDefault().AddSeconds(expiryInstanceCleanup))
                {
                    logger.Info($"Expired deployment {key} deleted on {DateTime.UtcNow}");
                    pool.Delete(key, true);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.Info("Watchdog service is stopping.");

            timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            timer?.Dispose();
        }
    }
}