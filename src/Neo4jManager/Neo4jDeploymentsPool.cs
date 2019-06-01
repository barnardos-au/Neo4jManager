using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace Neo4jManager
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Neo4jDeploymentsPool : ConcurrentDictionary<string, INeo4jInstance>, INeo4jDeploymentsPool
    {
        private static readonly object _object = new object();
        
        private readonly INeo4jManagerConfig neo4JManagerConfig;
        private readonly INeo4jInstanceFactory neo4jInstanceFactory;

        public Neo4jDeploymentsPool(
            INeo4jManagerConfig neo4JManagerConfig,
            INeo4jInstanceFactory neo4jInstanceFactory)
        {
            this.neo4JManagerConfig = neo4JManagerConfig;
            this.neo4jInstanceFactory = neo4jInstanceFactory;
        }

        public string Create(Neo4jDeploymentRequest request)
        {
            var id = Guid.NewGuid().ToString();
            
            Helper.Download(request.Version, neo4JManagerConfig.Neo4jBasePath);
            Helper.Extract(request.Version, neo4JManagerConfig.Neo4jBasePath);

            var deploymentFolderName = Helper.GenerateValidFolderName(id);
            if (string.IsNullOrEmpty(deploymentFolderName)) throw new ArgumentException("Error creating folder with given Id");

            var targetDeploymentPath = Path.Combine(neo4JManagerConfig.Neo4jBasePath, deploymentFolderName);
            Helper.SafeDelete(targetDeploymentPath);
            Helper.CopyDeployment(request.Version, neo4JManagerConfig.Neo4jBasePath, targetDeploymentPath);

            request.Neo4jFolder = Directory.GetDirectories(targetDeploymentPath)
                .First(f => f.Contains(request.Version.Version, StringComparison.OrdinalIgnoreCase));

            lock (_object)
            {
                request.Endpoints = new Neo4jEndpoints
                {
                    HttpEndpoint = new Uri($"http://localhost:{neo4JManagerConfig.StartHttpPort + Count}"),
                    BoltEndpoint = new Uri($"bolt://localhost:{neo4JManagerConfig.StartBoltPort + Count}"),
                };

                var instance = neo4jInstanceFactory.Create(request);

                TryAdd(id, instance);

                return id;
            }
        }

        public void Delete(string id)
        {
            var instance = this[id];
            instance.Dispose();

            TryRemove(id, out instance);
        }

        public void DeleteAll()
        {
            foreach (var instance in Values)
            {
                instance.Dispose();
            }

            Clear();
        }

        public void Dispose()
        {
            DeleteAll();
        }
    }
}
