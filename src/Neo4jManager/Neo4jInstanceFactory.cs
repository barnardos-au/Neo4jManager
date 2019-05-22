using System;
using System.Diagnostics.CodeAnalysis;
using Neo4jManager.V3;

namespace Neo4jManager
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Neo4jInstanceFactory : INeo4jInstanceFactory
    {
        private readonly IFileCopy fileCopy;
        private readonly IJavaResolver javaResolver;
        private string javaPath;

        public Neo4jInstanceFactory(
            IFileCopy fileCopy,
            IJavaResolver javaResolver)
        {
            this.fileCopy = fileCopy;
            this.javaResolver = javaResolver;
        }

        private string JavaPath => javaPath ?? (javaPath = javaResolver.GetJavaPath());

        public INeo4jInstance Create(string neo4jFolder, Neo4jVersion neo4jVersion, Neo4jEndpoints endpoints)
        {
            INeo4jInstance instance;

            switch (neo4jVersion.Architecture)
            {
                case Neo4jArchitecture.V3:
                    instance = new Neo4jV3JavaInstanceProvider(JavaPath, neo4jFolder, fileCopy, neo4jVersion, endpoints);

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
