﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Neo4jManager
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Neo4jDeploymentsPool : INeo4jDeploymentsPool
    {
        private readonly INeo4jManagerConfig neo4JManagerConfig;
        private readonly INeo4jInstanceFactory neo4jInstanceFactory;

        public Neo4jDeploymentsPool(
            INeo4jManagerConfig neo4JManagerConfig,
            INeo4jInstanceFactory neo4jInstanceFactory)
        {
            this.neo4JManagerConfig = neo4JManagerConfig;
            this.neo4jInstanceFactory = neo4jInstanceFactory;

            Deployments = new Dictionary<string, INeo4jInstance>();
        }

        public INeo4jInstance Create(Neo4jVersion neo4jVersion, string id)
        {
            Helper.Download(neo4jVersion, neo4JManagerConfig.Neo4jBasePath);
            Helper.Extract(neo4jVersion, neo4JManagerConfig.Neo4jBasePath);

            var targetDeploymentPath = Path.Combine(neo4JManagerConfig.Neo4jBasePath, id);
            Helper.CopyDeployment(neo4jVersion, neo4JManagerConfig.Neo4jBasePath, targetDeploymentPath);

            var endpoints = new Neo4jEndpoints
            {
                HttpEndpoint = new Uri($"http://localhost:{neo4JManagerConfig.StartHttpPort + Deployments.Count}"),
            };

            if (neo4jVersion.Architecture != Neo4jArchitecture.V2)
            {
                endpoints.BoltEndpoint = new Uri($"bolt://localhost:{neo4JManagerConfig.StartBoltPort + Deployments.Count}");
            }

            var neo4jFolder = Directory.GetDirectories(targetDeploymentPath)[0];

            var instance = neo4jInstanceFactory.Create(neo4jFolder, neo4jVersion, endpoints);

            Deployments.Add(id, instance);

            return instance;
        }

        public void Delete(string id)
        {
            var instance = Deployments[id];
            instance.Dispose();
            Deployments.Remove(id);
        }

        public void DeleteAll()
        {
            foreach (var instance in Deployments.Values)
            {
                instance.Dispose();
            }

            Deployments.Clear();
        }

        public Dictionary<string, INeo4jInstance> Deployments { get; }

        public void Dispose()
        {
            DeleteAll();
        }
    }
}