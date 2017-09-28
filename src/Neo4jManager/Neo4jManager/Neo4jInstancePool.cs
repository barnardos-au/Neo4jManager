using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Neo4jManager
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Neo4jInstancePool : INeo4jInstancePool
    {
        private readonly INeo4jManagerConfig neo4JManagerConfig;
        private readonly INeo4jInstanceFactory neo4jInstanceFactory;

        public Neo4jInstancePool(
            INeo4jManagerConfig neo4JManagerConfig,
            INeo4jInstanceFactory neo4jInstanceFactory)
        {
            this.neo4JManagerConfig = neo4JManagerConfig;
            this.neo4jInstanceFactory = neo4jInstanceFactory;

            Instances = new Dictionary<string, INeo4jInstance>();
        }

        public INeo4jInstance Create(Neo4jVersion neo4jVersion, string id)
        {
            Helper.Download(neo4jVersion, neo4JManagerConfig.Neo4jBasePath);
            Helper.Extract(neo4jVersion, neo4JManagerConfig.Neo4jBasePath);

            var targetDeploymentPath = Path.Combine(neo4JManagerConfig.Neo4jBasePath, id);
            Helper.CopyDeployment(neo4jVersion, neo4JManagerConfig.Neo4jBasePath, targetDeploymentPath);

            var instance = neo4jInstanceFactory.Create(neo4jVersion, targetDeploymentPath, Instances.Count);

            Instances.Add(id, instance);

            return instance;
        }

        public void Reset()
        {
            foreach (var instance in Instances.Values)
            {
                instance.Dispose();
            }

            Instances.Clear();
        }

        public Dictionary<string, INeo4jInstance> Instances { get; }

        public void Dispose()
        {
            Reset();
        }
    }
}
