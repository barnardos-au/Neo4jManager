using System;
using System.Linq;
using System.Net;
using Neo4jManager.ServiceModel.Deployments;
using ServiceStack;

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
            var response = new DeploymentResponse
            {
                Deployment = pool
                    .SingleOrDefault(p => p.Key == request.Id)
                    .ConvertTo<Deployment>()
            };

            return response;
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

            pool.Create(neo4jVersion, request.Id);

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
