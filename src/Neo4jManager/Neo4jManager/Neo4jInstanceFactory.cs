using System;
using System.Diagnostics.CodeAnalysis;
using Neo4jManager.V2;
using Neo4jManager.V3;

namespace Neo4jManager
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Neo4jInstanceFactory : INeo4jInstanceFactory
    {
        private readonly IFileCopy fileCopy;
        private readonly string javaPath;

        public Neo4jInstanceFactory(IFileCopy fileCopy)
        {
            this.fileCopy = fileCopy;

            javaPath = Helper.FindJavaExe();
        }

        public INeo4jInstance Create(string neo4jFolder, Neo4jVersion neo4jVersion, Neo4jEndpoints endpoints)
        {
            INeo4jInstance instance;

            switch (neo4jVersion.Architecture)
            {
                case Neo4jArchitecture.V2:
                    instance = new Neo4jV2ServiceInstanceProvider(neo4jFolder, fileCopy, neo4jVersion, endpoints);

                    instance.Configure(Neo4jV2ProcessBasedInstanceProvider.Neo4jServierPropertiesConfigFile, "dbms.security.auth_enabled", "false");
                    instance.Configure(Neo4jV2ProcessBasedInstanceProvider.Neo4jPropertiesConfigFile, "allow_store_upgrade", "true");
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
                    instance = new Neo4jV3JavaInstanceProvider(javaPath, neo4jFolder, fileCopy, neo4jVersion, endpoints);

                    const string configFile = Neo4jV3ProcessBasedInstanceProvider.Neo4jConfigFile;
                    instance.Configure(configFile, "dbms.security.auth_enabled", "false");
                    instance.Configure(configFile, "dbms.allow_format_migration", "true");
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
