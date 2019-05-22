using ServiceStack;
using System;
using System.Collections.Generic;
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
            
            var keyedInstance = pool.SingleOrDefault(p => p.Key == request.Id);

            return new DeploymentResponse { Deployment = keyedInstance.ConvertTo<Deployment>() };
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

            return new DeploymentResponse
            {
                Deployment = new KeyValuePair<string, INeo4jInstance>(request.Id, instance).ConvertTo<Deployment>()
            };
        }

        // Delete
        public object Delete(DeploymentRequest request)
        {
            if (!pool.ContainsKey(request.Id))
                return new HttpResult(HttpStatusCode.NotFound);

            var keyedInstance = pool.Single(p => p.Key == request.Id);
            
            pool.Delete(request.Id);

            return new DeploymentResponse
            {
                Deployment = keyedInstance.ConvertTo<Deployment>()
            };
        }
    }
}
