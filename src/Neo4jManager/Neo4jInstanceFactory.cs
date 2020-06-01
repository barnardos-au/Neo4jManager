using System;
using System.Diagnostics.CodeAnalysis;
using Neo4jManager.V3;

namespace Neo4jManager
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Neo4jInstanceFactory : INeo4jInstanceFactory
    {
        public const string Neo4jConfigFile = "neo4j.conf";

        private readonly Func<Neo4jDeploymentRequest, Neo4jV3JavaInstanceProvider> javaInstanceFunc;

        public Neo4jInstanceFactory(Func<Neo4jDeploymentRequest, Neo4jV3JavaInstanceProvider> javaInstanceFunc)
        {
            this.javaInstanceFunc = javaInstanceFunc;
        }

        public INeo4jInstance Create(Neo4jDeploymentRequest request)
        {
            INeo4jInstance instance;

            switch (request.Version.Architecture)
            {
                case Neo4jArchitecture.V3:
                    instance = javaInstanceFunc(request);

                    const string configFile = Neo4jConfigFile;
                    instance.Configure(configFile, "dbms.security.auth_enabled", "false");
                    instance.Configure(configFile, "dbms.allow_format_migration", "true");
                    instance.Configure(configFile, "dbms.directories.import", null);

                    instance.Configure(configFile, "dbms.connector.http.listen_address", $":{request.Endpoints.HttpEndpoint.Port}");

                    if (request.Endpoints.BoltEndpoint != null)
                    {
                        instance.Configure(configFile, "dbms.connector.bolt.listen_address", $":{request.Endpoints.BoltEndpoint.Port}");
                        instance.Configure(configFile, "dbms.connector.bolt.enabled", "true");
                    }
                    else
                    {
                        instance.Configure(configFile, "dbms.connector.bolt.enabled", "false");
                    }

                    if (request.Endpoints.HttpsEndpoint != null)
                    {
                        instance.Configure(configFile, "dbms.connector.https.listen_address", $":{request.Endpoints.HttpsEndpoint.Port}");
                        instance.Configure(configFile, "dbms.connector.https.enabled", "true");
                    }
                    else
                    {
                        instance.Configure(configFile, "dbms.connector.https.enabled", "false");
                    }
                    break;

                default:
                    throw new ArgumentException($"Architecture: {request.Version.Architecture} unknown");
            }

            return instance;
        }
    }
}
