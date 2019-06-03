using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Neo4jManager.ServiceModel;
using ServiceStack;

namespace Neo4jManager.Client
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Neo4jManagerClient : INeo4jManagerClient
    {
        private readonly IServiceClient client;
        
        public Neo4jManagerClient(string baseUrl)
        {
            client = new JsonServiceClient(baseUrl);
        }

        public IEnumerable<Version> GetVersions()
        {
            return client.Get(new VersionsRequest()).Versions;
        }

        public async Task<IEnumerable<Version>> GetVersionsAsync()
        {
            return (await client.GetAsync(new VersionsRequest())).Versions;
        }

        public IEnumerable<Deployment> GetDeployments()
        {
            return client.Get(new DeploymentsRequest()).Deployments;
        }

        public async Task<IEnumerable<Deployment>> GetDeploymentsAsync()
        {
            return (await client.GetAsync(new DeploymentsRequest())).Deployments;
        }

        public Deployment GetDeployment(string id)
        {
            return client.Get(new DeploymentRequest
            {
                Id = id,
            }).Deployment;
        }

        public async Task<Deployment> GetDeploymentAsync(string id)
        {
            return (await client.GetAsync(new DeploymentRequest
            {
                Id = id,
            })).Deployment;
        }

        public void DeleteAll()
        {
            client.Delete(new DeploymentsRequest());
        }

        public async Task DeleteAllAsync()
        {
            await client.DeleteAsync(new DeploymentsRequest());
        }

        public Deployment Create(string versionNumber, List<Setting> settings = null, List<string> pluginUrls = null)
        {
            return client.Post(new CreateDeploymentRequest
            {
                Version = versionNumber,
                Settings = settings,
                PluginUrls = pluginUrls
            }).Deployment;
        }

        public async Task<Deployment> CreateAsync(string versionNumber, List<Setting> settings = null, List<string> pluginUrls = null)
        {
            return (await client.PostAsync(new CreateDeploymentRequest
            {
                Version = versionNumber,
                Settings = settings,
                PluginUrls = pluginUrls
            })).Deployment;
        }

        public void Delete(string id)
        {
            client.Delete(new DeploymentRequest
            {
                Id = id,
            });
        }

        public async Task DeleteAsync(string id)
        {
            await client.DeleteAsync(new DeploymentRequest
            {
                Id = id,
            });
        }

        public void Start(string id)
        {
            client.Post(new ControlRequest
            {
                Id = id,
                Operation = Operation.Start
            });
        }

        public async Task StartAsync(string id)
        {
            await client.PostAsync(new ControlRequest
            {
                Id = id,
                Operation = Operation.Start
            });
        }

        public void Stop(string id)
        {
            client.Post(new ControlRequest
            {
                Id = id,
                Operation = Operation.Stop
            });
        }

        public async Task StopAsync(string id)
        {
            await client.PostAsync(new ControlRequest
            {
                Id = id,
                Operation = Operation.Stop
            });
        }

        public void Restart(string id)
        {
            client.Post(new ControlRequest
            {
                Id = id,
                Operation = Operation.Restart
            });
        }

        public async Task RestartAsync(string id)
        {
            await client.PostAsync(new ControlRequest
            {
                Id = id,
                Operation = Operation.Restart
            });
        }

        public void Clear(string id)
        {
            client.Post(new ControlRequest
            {
                Id = id,
                Operation = Operation.Clear
            });
        }

        public async Task ClearAsync(string id)
        {
            await client.PostAsync(new ControlRequest
            {
                Id = id,
                Operation = Operation.Clear
            });
        }

        public void Backup(string id)
        {
            client.Post(new ControlRequest
            {
                Id = id,
                Operation = Operation.Backup
            });
        }

        public async Task BackupAsync(string id)
        {
            await client.PostAsync(new ControlRequest
            {
                Id = id,
                Operation = Operation.Backup
            });
        }

        public void Restore(string id, string sourcePath)
        {
            client.Post(new ControlRequest
            {
                Id = id,
                SourcePath = sourcePath,
                Operation = Operation.Restore
            });
        }

        public async Task RestoreAsync(string id, string sourcePath)
        {
            await client.PostAsync(new ControlRequest
            {
                Id = id,
                SourcePath = sourcePath,
                Operation = Operation.Restore
            });
        }

        public void Configure(string id, string configFile, string key, string value)
        {
            client.Post(new ControlRequest
            {
                Id = id,
                Setting = new Setting
                {
                    ConfigFile = configFile,
                    Key = key,
                    Value = value
                },
                Operation = Operation.Configure
            });
        }

        public async Task ConfigureAsync(string id, string configFile, string key, string value)
        {
            await client.PostAsync(new ControlRequest
            {
                Id = id,
                Setting = new Setting
                {
                    ConfigFile = configFile,
                    Key = key,
                    Value = value
                },
                Operation = Operation.Configure
            });
        }
    }
}
