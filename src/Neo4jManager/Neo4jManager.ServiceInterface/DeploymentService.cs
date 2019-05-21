using ServiceStack;
using System;
using System.Linq;
using System.Net;
using Neo4jManager.ServiceModel;
using ServiceStack.Configuration;

namespace Neo4jManager.ServiceInterface
{
    // ReSharper disable once InconsistentNaming
    public class DeploymentService : Service
    {
        private readonly INeo4jDeploymentsPool pool;
        private readonly IAppSettings appSettings;

        public DeploymentService(
            INeo4jDeploymentsPool pool,
            IAppSettings appSettings)
        {
            this.pool = pool;
            this.appSettings = appSettings;
        }

        // Get by Id
        public object Get(DeploymentRequest request)
        {
            if (!pool.ContainsKey(request.Id))
                return new HttpResult(HttpStatusCode.NotFound);
            
            var deployment = pool[request.Id];

            return new DeploymentResponse { Deployment = deployment.ConvertTo<Deployment>() };
        }

        // Create
        public object Post(DeploymentRequest request)
        {
            var version = appSettings.Neo4jVersions()
                .Single(v => v.VersionNumber == request.Version);
            var neo4jVersion = new Neo4jVersion
            {
                Architecture = (Neo4jArchitecture)Enum.Parse(typeof(Neo4jArchitecture), version.Architecture),
                DownloadUrl = version.DownloadUrl,
                Version = version.VersionNumber,
                ZipFileName = version.ZipFileName
            };

            var instance = pool.Create(neo4jVersion, request.Id);

            if (request.Settings != null)
            {
                foreach (var setting in request.Settings)
                {
                    instance.Configure(setting.ConfigFile, setting.Key, setting.Value);
                }
            }

            return new DeploymentResponse { Deployment = instance.ConvertTo<Deployment>() };
        }

        // Delete
        public DeploymentResponse Delete(DeploymentRequest request)
        {
            var instance = pool[request.Id];
            
            pool.Delete(request.Id);

            return new DeploymentResponse
            {
                Deployment = instance.ConvertTo<Deployment>()
            };
        }
    }
}
