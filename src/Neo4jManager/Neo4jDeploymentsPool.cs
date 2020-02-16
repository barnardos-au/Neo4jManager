using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using ServiceStack.Configuration;

namespace Neo4jManager
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Neo4jDeploymentsPool : ConcurrentDictionary<string, INeo4jInstance>, INeo4jDeploymentsPool
    {
        private readonly IAppSettings appSettings;
        private static readonly object _object = new object();
        
        private readonly INeo4jInstanceFactory neo4jInstanceFactory;

        public Neo4jDeploymentsPool(
            IAppSettings appSettings,
            INeo4jInstanceFactory neo4jInstanceFactory)
        {
            this.appSettings = appSettings;
            this.neo4jInstanceFactory = neo4jInstanceFactory;
        }

        public string Create(Neo4jDeploymentRequest request)
        {
            var id = Guid.NewGuid().ToString();
            
            Helper.Download(request.Version, appSettings.GetString(AppSettingsKeys.Neo4jBasePath));
            Helper.Extract(request.Version, appSettings.GetString(AppSettingsKeys.Neo4jBasePath));

            var targetDeploymentPath = GetDeploymentPath(id);
            Helper.SafeDelete(targetDeploymentPath);
            Helper.CopyDeployment(request.Version, appSettings.GetString(AppSettingsKeys.Neo4jBasePath), targetDeploymentPath);

            request.Neo4jFolder = Directory.GetDirectories(targetDeploymentPath)
                .First(f => f.Contains(request.Version.Version, StringComparison.OrdinalIgnoreCase));

            lock (_object)
            {
                short offset = 0;
                while (true)
                {
                    var instances = this.Where(i => i.Value.Offset == offset).Select(i => i.Value).ToList();
                    if (instances.Count == 0 || instances.All(i => i.Status == Status.Deleted)) break;

                    offset++;
                }

                request.Offset = offset;

                request.Endpoints = new Neo4jEndpoints
                {
                    HttpEndpoint = new Uri($"http://localhost:{appSettings.Get<long>(AppSettingsKeys.StartHttpPort) + offset}"),
                    BoltEndpoint = new Uri($"bolt://localhost:{appSettings.Get<long>(AppSettingsKeys.StartBoltPort) + offset}"),
                };

                var instance = neo4jInstanceFactory.Create(request);

                TryAdd(id, instance);

                return id;
            }
        }

        public void Delete(string id, bool permanent)
        {
            var instance = this[id];
            if (instance.Status != Status.Deleted)
            {
                instance.Dispose();
            }

            if (!permanent) return;
            
            var targetDeploymentPath = GetDeploymentPath(id);
            Helper.SafeDelete(targetDeploymentPath);
            
            TryRemove(id, out _);
        }

        public void DeleteAll(bool permanent)
        {
            foreach (var instance in Values.Where(i => i.Status != Status.Deleted))
            {
                instance.Dispose();
            }

            if (!permanent) return;

            foreach (var key in Keys)
            {
                var targetDeploymentPath = GetDeploymentPath(key);
                Helper.SafeDelete(targetDeploymentPath);
            }

            Clear();
        }

        public void Dispose()
        {
            DeleteAll(true);
        }

        private string GetDeploymentPath(string id)
        {
            var deploymentFolderName = Helper.GenerateValidFolderName(id);
            if (string.IsNullOrEmpty(deploymentFolderName)) throw new ArgumentException("Error creating folder with given Id");

           return Path.Combine(appSettings.DeploymentsBasePath(), deploymentFolderName);
        }
    }
}
