using Neo4jManager.ServiceModel.Deployments;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Neo4jManager.ServiceInterface
{
    // ReSharper disable once InconsistentNaming
    public class DeploymentService : Service
    {
        private readonly INeo4jDeploymentsPool pool;
        private readonly INeo4jVersionRepository repository;

        public DeploymentService(
            INeo4jDeploymentsPool pool,
            INeo4jVersionRepository repository)
        {
            this.pool = pool;
            this.repository = repository;
        }

        // Get by Id
        public DeploymentResponse Get(DeploymentRequest request)
        {
            var deployment = pool.SingleOrDefault(p => p.Key == request.Id);
            if (deployment.Equals(default(KeyValuePair<string, int>)) || string.IsNullOrEmpty(deployment.Key))
            {
                return new DeploymentResponse();
            }

            return new DeploymentResponse { Deployment = deployment.Value.ConvertTo<Deployment>() };
        }

        // Create

        public object Post(DeploymentRequest request)
        {
            var version = repository.GetVersions()
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

            return this.Redirect($"/deployment/{request.Id}");
        }

        // Delete
        public object Delete(DeploymentRequest request)
        {
            pool.Delete(request.Id);

            return new HttpResult(HttpStatusCode.NoContent);
        }
    }
}
