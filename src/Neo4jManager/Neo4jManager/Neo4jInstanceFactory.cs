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
                    instance = new Neo4jV2ServiceInstanceProvider(neo4jFolder, fileCopy, endpoints);

                    instance.Configure(Neo4jV2ProcessBasedInstanceProvider.Neo4jServierPropertiesConfigFile, "dbms.security.auth_enabled", "false");
                    instance.Configure(Neo4jV2ProcessBasedInstanceProvider.Neo4jPropertiesConfigFile, "allow_store_upgrade", "true");
                    instance.Configure(Neo4jV2ProcessBasedInstanceProvider.Neo4jWrapperConfigFile, "wrapper.java.additional.1", "-Dfile.encoding=UTF-8");
                    instance.Configure(Neo4jV2ProcessBasedInstanceProvider.Neo4jWrapperConfigFile, "wrapper.name", $"Neo4j{endpoints.HttpEndpoint.Port}");

                    instance.Configure(Neo4jV2ProcessBasedInstanceProvider.Neo4jServierPropertiesConfigFile, "org.neo4j.server.webserver.port", $"{endpoints.HttpEndpoint.Port}");

                    if (endpoints.HttpsEndpoint != null)
                    {
                        instance.Configure(Neo4jV2ProcessBasedInstanceProvider.Neo4jServierPropertiesConfigFile, "org.neo4j.server.webserver.https.port", $"{endpoints.HttpsEndpoint.Port}");
                        instance.Configure(Neo4jV2ProcessBasedInstanceProvider.Neo4jServierPropertiesConfigFile, "org.neo4j.server.webserver.https.enabled", "true");
                    }
                    else
                    {
                        instance.Configure(Neo4jV2ProcessBasedInstanceProvider.Neo4jServierPropertiesConfigFile, "org.neo4j.server.webserver.https.enabled", "false");
                    }

                    break;

                case Neo4jArchitecture.V3:
                    instance = new Neo4jV3JavaInstanceProvider(javaPath, neo4jFolder, fileCopy, endpoints);

                    const string configFile = Neo4jV3ProcessBasedInstanceProvider.Neo4jConfigFile;
                    instance.Configure(configFile, "dbms.security.auth_enabled", "false");
                    instance.Configure(configFile, "dbms.allow_format_migration", "true");
                    instance.Configure(configFile, "dbms.jvm.additional.1", "-Dfile.encoding=UTF-8");
                    instance.Configure(configFile, "dbms.directories.import", "");

                    instance.Configure(configFile, "dbms.connector.http.listen_address", $":{endpoints.HttpEndpoint.Port}");

                    if (endpoints.BoltEndpoint != null)
                    {
                        instance.Configure(configFile, "dbms.connector.bolt.listen_address", $":{endpoints.BoltEndpoint.Port}");
                        instance.Configure(configFile, "dbms.connector.bolt.enabled", "true");
                    }
                    else
                    {
                        instance.Configure(configFile, "dbms.connector.bolt.enabled", "false");
                    }

                    if (endpoints.HttpsEndpoint != null)
                    {
                        instance.Configure(configFile, "dbms.connector.https.listen_address", $":{endpoints.HttpsEndpoint.Port}");
                        instance.Configure(configFile, "dbms.connector.https.enabled", "true");
                    }
                    else
                    {
                        instance.Configure(configFile, "dbms.connector.https.enabled", "false");
                    }

                    break;

                default:
                    throw new ArgumentException($"Architecture: {neo4jVersion.Architecture} unknown");
            }

            return instance;
        }
    }
}
