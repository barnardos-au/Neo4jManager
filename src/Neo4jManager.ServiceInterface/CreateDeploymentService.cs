using ServiceStack;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Neo4jManager.ServiceModel;
using ServiceStack.Configuration;

namespace Neo4jManager.ServiceInterface
{
    // ReSharper disable once InconsistentNaming
    public class CreateDeploymentService : Service
    {
        private readonly INeo4jDeploymentsPool pool;
        private readonly IAppSettings appSettings;

        public CreateDeploymentService(
            INeo4jDeploymentsPool pool,
            IAppSettings appSettings)
        {
            this.pool = pool;
            this.appSettings = appSettings;
        }

        // Create
        public async Task<DeploymentResponse> Post(CreateDeploymentRequest request)
        {
            var version = appSettings.Neo4jVersions()
                .Single(v => v.VersionNumber == request.Version);

            var defaultLeasePeriod = appSettings.Get<TimeSpan>(AppSettingsKeys.DefaultLeasePeriod);

            var neo4jDeploymentRequest = new Neo4jDeploymentRequest
            {
                LeasePeriod = request.LeasePeriod ?? defaultLeasePeriod,
                Version = new Neo4jVersion
                {
                    Architecture = (Neo4jArchitecture) Enum.Parse(typeof(Neo4jArchitecture), version.Architecture),
                    DownloadUrl = version.DownloadUrl,
                    Version = version.VersionNumber,
                    ZipFileName = version.ZipFileName
                }
            };
            
            var id = pool.Create(neo4jDeploymentRequest);

            if (!pool.ContainsKey(id))
                throw HttpError.NotFound($"Deployment {id} not found");

            var instance = pool[id];

            request.PluginUrls?.ForEach(p =>
            {
                if (!p.IsEmpty())
                {
                    instance.DownloadPlugin(p);
                }
            });

            request.Settings?.ForEach(s =>
            {
                instance.Configure(s.ConfigFile, s.Key, s.Value);
            });

            if (!string.IsNullOrEmpty(request.RestoreDumpFileUrl))
            {
                // Force start to create initial databases/graph.db folder
                await instance.Start(CancellationToken.None);
                
                string tempFile;
                
                if (Uri.IsWellFormedUriString(request.RestoreDumpFileUrl, UriKind.Absolute))
                {
                    tempFile = await Neo4jManager.Helper.DownloadFileAsync(
                        request.RestoreDumpFileUrl, 
                        Path.GetTempPath());
                }
                else
                {
                    tempFile = request.RestoreDumpFileUrl;
                }

                await instance.Restore(CancellationToken.None, tempFile);
            }
        
            if (request.AutoStart)
            {
                await instance.Start(CancellationToken.None);
            }
            
            var keyedInstance = pool.SingleOrDefault(p => p.Key == id);
            
            return keyedInstance.ConvertTo<DeploymentResponse>();
        }
    }
}
