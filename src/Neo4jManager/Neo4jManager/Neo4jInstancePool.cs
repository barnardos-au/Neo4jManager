using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Neo4jManager
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Neo4jInstancePool : IDisposable
    {
        private readonly IFileCopy fileCopy;
        private readonly INeo4jManagerConfig neo4JManagerConfig;

        public Neo4jInstancePool(IFileCopy fileCopy, INeo4jManagerConfig neo4JManagerConfig)
        {
            this.fileCopy = fileCopy;
            this.neo4JManagerConfig = neo4JManagerConfig;

            Pool = new Dictionary<string, INeo4jInstance>();
        }

        public INeo4jInstance Create(Neo4jVersion neo4jVersion, string id)
        {
            Helper.Download(neo4jVersion, neo4JManagerConfig.Neo4jBasePath);
            Helper.Extract(neo4jVersion, neo4JManagerConfig.Neo4jBasePath);

            var targetDeploymentPath = Path.Combine(neo4JManagerConfig.Neo4jBasePath, id);
            Helper.CopyDeployment(neo4jVersion, neo4JManagerConfig.Neo4jBasePath, targetDeploymentPath);
            var neo4jFolder = Directory.GetDirectories(targetDeploymentPath)[0];

            var offset = Pool.Count;

            var endpoints = new Neo4jEndpoints
            {
                HttpEndpoint = new Uri($"http://localhost:{neo4JManagerConfig.StartHttpPort + offset}"),
                BoltEndpoint = new Uri($"bolt://localhost:{neo4JManagerConfig.StartBoltPort + offset}")
            };

            var instance = new JavaInstanceProviderV3(Helper.FindJavaExe(), neo4jFolder, fileCopy, endpoints)
            {
                Id = id
            };
            
            instance.Configure("", "dbms.security.auth_enabled", "false");
            instance.Configure("", "dbms.allow_format_migration", "true");
            instance.Configure("", "dbms.jvm.additional.1", "-Dfile.encoding=UTF-8");
            instance.Configure("", "dbms.directories.import", "");
            instance.Configure("", "dbms.connector.http.listen_address", $":{endpoints.HttpEndpoint.Port}");
            instance.Configure("", "dbms.connector.bolt.listen_address", $":{endpoints.BoltEndpoint.Port}");
            instance.Configure("", "dbms.connector.https.enabled", "false");

            Pool.Add(id, instance);

            return instance;
        }

        public Dictionary<string, INeo4jInstance> Pool { get; }

        public void Dispose()
        {
            foreach (var instance in Pool.Values)
            {
                instance.Dispose();
            }
        }
    }
}
