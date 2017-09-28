using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Neo4jManager.V2;
using Neo4jManager.V3;

namespace Neo4jManager
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Neo4jInstanceFactory : INeo4jInstanceFactory
    {
        private readonly INeo4jManagerConfig neo4JManagerConfig;
        private readonly IFileCopy fileCopy;

        public Neo4jInstanceFactory(
            INeo4jManagerConfig neo4JManagerConfig,
            IFileCopy fileCopy)
        {
            this.neo4JManagerConfig = neo4JManagerConfig;
            this.fileCopy = fileCopy;
        }

        public INeo4jInstance Create(Neo4jVersion neo4jVersion, string targetDeploymentPath, int portOffset)
        {
            var endpoints = new Neo4jEndpoints
            {
                HttpEndpoint = new Uri($"http://localhost:{neo4JManagerConfig.StartHttpPort + portOffset}"),
            };

            if (neo4jVersion.Architecture != Neo4jArchitecture.V2)
            {
                endpoints.BoltEndpoint = new Uri($"bolt://localhost:{neo4JManagerConfig.StartBoltPort + portOffset}");
            }

            var javaPath = Helper.FindJavaExe();
            var neo4jFolder = Directory.GetDirectories(targetDeploymentPath)[0];

            INeo4jInstance instance;

            switch (neo4jVersion.Architecture)
            {
                case Neo4jArchitecture.V2:
                    instance = new Neo4jV2JavaInstanceProvider(javaPath, neo4jFolder, fileCopy, endpoints);
                    break;

                case Neo4jArchitecture.V3:
                    instance = new Neo4jV3JavaInstanceProvider(javaPath, neo4jFolder, fileCopy, endpoints);

                    instance.Configure("", "dbms.security.auth_enabled", "false");
                    instance.Configure("", "dbms.allow_format_migration", "true");
                    instance.Configure("", "dbms.jvm.additional.1", "-Dfile.encoding=UTF-8");
                    instance.Configure("", "dbms.directories.import", "");

                    instance.Configure("", "dbms.connector.http.listen_address", $":{endpoints.HttpEndpoint.Port}");

                    if (endpoints.BoltEndpoint != null)
                    {
                        instance.Configure("", "dbms.connector.bolt.listen_address", $":{endpoints.BoltEndpoint.Port}");
                        instance.Configure("", "dbms.connector.bolt.enabled", "true");
                    }
                    else
                    {
                        instance.Configure("", "dbms.connector.bolt.enabled", "false");
                    }

                    if (endpoints.HttpsEndpoint != null)
                    {
                        instance.Configure("", "dbms.connector.https.listen_address", $":{endpoints.HttpsEndpoint.Port}");
                        instance.Configure("", "dbms.connector.https.enabled", "true");
                    }
                    else
                    {
                        instance.Configure("", "dbms.connector.https.enabled", "false");
                    }

                    break;

                default:
                    throw new ArgumentException($"Architecture: {neo4jVersion.Architecture} unknown");
            }

            return instance;
        }
    }
}
