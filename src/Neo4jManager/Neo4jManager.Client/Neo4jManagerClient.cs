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
        private readonly IJsonServiceClient client;
        
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
                Id = id
            }).Deployment;
        }

        public async Task<Deployment> GetDeploymentAsync(string id)
        {
            return (await client.GetAsync(new DeploymentRequest
            {
                Id = id
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

        public Deployment Create(string id, string versionNumber)
        {
            return client.Post(new DeploymentRequest
            {
                Id = id,
                Version = versionNumber
            }).Deployment;
        }

        public async Task<Deployment> CreateAsync(string id, string versionNumber)
        {
            return (await client.PostAsync(new DeploymentRequest
            {
                Id = id,
                Version = versionNumber
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

        public void Backup(string id, string destinationPath, bool stopInstanceBeforeBackup = true)
        {
            client.Post(new ControlRequest
            {
                Id = id,
                DestinationPath = destinationPath,
                StopInstanceBeforeBackup = stopInstanceBeforeBackup,
                Operation = Operation.Backup
            });
        }

        public async Task BackupAsync(string id, string destinationPath, bool stopInstanceBeforeBackup = true)
        {
            await client.PostAsync(new ControlRequest
            {
                Id = id,
                DestinationPath = destinationPath,
                StopInstanceBeforeBackup = stopInstanceBeforeBackup,
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

//        public void Configure(string id, string configFile, string key, string value)
//        {
//            $"{baseUrl}/deployments/{id}/config"
//                .PostJsonToUrl(new Config { ConfigFile = configFile, Key = key, Value = value });
//        }
//
//        public async Task ConfigureAsync(string id, string configFile, string key, string value)
//        {
//            await $"{baseUrl}/deployments/{id}/config"
//                .PostJsonToUrlAsync(new Config { ConfigFile = configFile, Key = key, Value = value });
//        }
    }
}
