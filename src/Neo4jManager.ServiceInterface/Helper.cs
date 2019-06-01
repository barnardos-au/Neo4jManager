using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using Neo4jManager.ServiceModel;
using ServiceStack;
using Version = Neo4jManager.ServiceModel.Version;

namespace Neo4jManager.ServiceInterface
{
    public static class Helper
    {
        public static void KillJavaProcesses()
        {
            var searcher = new ManagementObjectSearcher(@"SELECT * FROM Win32_Process WHERE CommandLine like '%java%neo4j%'");
            var objects = searcher.Get();
            foreach (var o in objects)
            {
                Neo4jManager.Helper.SafeAction(() =>
                {
                    var id = Convert.ToInt32(o.GetPropertyValue("ProcessId"));
                    using (var p = Process.GetProcessById(id))
                    {
                        p.Kill();
                    }
                });
            }
        }
        
        public static void ConfigureMappers()
        {
            AutoMapping.RegisterConverter<Neo4jVersion, Version>(nv =>
            {
                var version = nv.ConvertTo<Version>(skipConverters:true);
                version.VersionNumber = nv.Version;

                return version;
            });

            AutoMapping.RegisterConverter<Neo4jEndpoints, Endpoints>(neo4jEndpoints =>
            {
                var endpoints = new Endpoints
                {
                    BoltEndpoint = neo4jEndpoints.BoltEndpoint?.AbsoluteUri,
                    HttpEndpoint = neo4jEndpoints.HttpEndpoint?.AbsoluteUri,
                    HttpsEndpoint = neo4jEndpoints.HttpsEndpoint?.AbsoluteUri
                };
                
                return endpoints;
            });

            AutoMapping.RegisterConverter<KeyValuePair<string, INeo4jInstance>, Deployment>(kvp =>
            {
                var neo4JDeployment = kvp.Value.Deployment;

                var deployment = neo4JDeployment.ConvertTo<Deployment>();
                deployment.Id = kvp.Key;
                deployment.Status = kvp.Value.Status.ToString();

                return deployment;
            });
            
            AutoMapping.RegisterConverter<KeyValuePair<string, INeo4jInstance>, DeploymentResponse>(kvp =>
            {
                var response = new DeploymentResponse
                {
                    Deployment = kvp.ConvertTo<Deployment>()
                };

                return response;
            });
        }
    }
}
