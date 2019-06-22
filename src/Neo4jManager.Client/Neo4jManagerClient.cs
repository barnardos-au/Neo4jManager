using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Neo4jManager.ServiceModel;
using ServiceStack;
using Version = Neo4jManager.ServiceModel.Version;

namespace Neo4jManager.Client
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Neo4jManagerClient : INeo4jManagerClient
    {
        private readonly JsonServiceClient client;
        
        public Neo4jManagerClient(string baseUrl)
        {
            client = new JsonServiceClient(baseUrl);
        }

        public IEnumerable<Version> GetVersions()
        {
            return GetVersions(new VersionsRequest());
        }

        public IEnumerable<Version> GetVersions(VersionsRequest request)
        {
            return client.Get(request).Versions;
        }

        public async Task<IEnumerable<Version>> GetVersionsAsync()
        {
            return await GetVersionsAsync(new VersionsRequest());
        }

        public async Task<IEnumerable<Version>> GetVersionsAsync(VersionsRequest request)
        {
            return (await client.GetAsync(request)).Versions;
        }

        public IEnumerable<Deployment> GetDeployments()
        {
            return GetDeployments(new DeploymentsRequest());
        }

        public IEnumerable<Deployment> GetDeployments(DeploymentsRequest request)
        {
            return client.Get(request).Deployments;
        }

        public async Task<IEnumerable<Deployment>> GetDeploymentsAsync()
        {
            return await GetDeploymentsAsync(new DeploymentsRequest());
        }

        public async Task<IEnumerable<Deployment>> GetDeploymentsAsync(DeploymentsRequest request)
        {
            return (await client.GetAsync(request)).Deployments;
        }

        public Deployment GetDeployment(string id)
        {
            return GetDeployment(new DeploymentRequest
            {
                Id = id,
            });
        }

        public Deployment GetDeployment(DeploymentRequest request)
        {
            return client.Get(request).Deployment;
        }

        public async Task<Deployment> GetDeploymentAsync(string id)
        {
            return await GetDeploymentAsync(new DeploymentRequest
            {
                Id = id,
            });
        }

        public async Task<Deployment> GetDeploymentAsync(DeploymentRequest request)
        {
            return (await client.GetAsync(request)).Deployment;
        }

        public void DeleteAll(bool permanent = false)
        {
            DeleteAll(new DeploymentsRequest { Permanent = permanent });
        }

        public void DeleteAll(DeploymentsRequest request)
        {
            client.Delete(request);
        }

        public async Task DeleteAllAsync(bool permanent = false)
        {
            await DeleteAllAsync(new DeploymentsRequest { Permanent = permanent });
        }

        public async Task DeleteAllAsync(DeploymentsRequest request)
        {
            await client.DeleteAsync(request);
        }

        public Deployment Create(
            string versionNumber, 
            TimeSpan? leasePeriod = null,
            List<Setting> settings = null, 
            List<string> pluginUrls = null, 
            string restoreDumpFile = null, 
            bool autoStart = false)
        {
            return Create(new CreateDeploymentRequest
            {
                Version = versionNumber,
                LeasePeriod = leasePeriod,
                Settings = settings,
                PluginUrls = pluginUrls,
                RestoreDumpFileUrl = restoreDumpFile,
                AutoStart = autoStart,
            });
        }

        public Deployment Create(CreateDeploymentRequest request)
        {
            return client.Post(request).Deployment;
        }

        public async Task<Deployment> CreateAsync(
            string versionNumber, 
            TimeSpan? leasePeriod = null,
            List<Setting> settings = null, 
            List<string> pluginUrls = null, 
            string restoreDumpFile = null, 
            bool autoStart = false)
        {
            return await CreateAsync(new CreateDeploymentRequest
            {
                Version = versionNumber,
                LeasePeriod = leasePeriod,
                Settings = settings,
                PluginUrls = pluginUrls,
                RestoreDumpFileUrl = restoreDumpFile,
                AutoStart = autoStart,
            });
        }

        public async Task<Deployment> CreateAsync(CreateDeploymentRequest request)
        {
            return (await client.PostAsync(request)).Deployment;
        }

        public void Delete(string id, bool permanent = false)
        {
            Delete(new DeploymentRequest
            {
                Id = id,
                Permanent = permanent
            });
        }

        public void Delete(DeploymentRequest request)
        {
            client.Delete(request);
        }

        public async Task DeleteAsync(string id, bool permanent = false)
        {
            await DeleteAsync(new DeploymentRequest
            {
                Id = id,
                Permanent = permanent
            });
        }

        public async Task DeleteAsync(DeploymentRequest request)
        {
            await client.DeleteAsync(request);
        }

        public void Start(string id)
        {
            Control(new ControlRequest
            {
                Id = id,
                Operation = Operation.Start
            });
        }

        public async Task StartAsync(string id)
        {
            await ControlAsync(new ControlRequest
            {
                Id = id,
                Operation = Operation.Start
            });
        }

        public void Stop(string id)
        {
            Control(new ControlRequest
            {
                Id = id,
                Operation = Operation.Stop
            });
        }

        public async Task StopAsync(string id)
        {
            await ControlAsync(new ControlRequest
            {
                Id = id,
                Operation = Operation.Stop
            });
        }


        public void Restart(string id)
        {
            Control(new ControlRequest
            {
                Id = id,
                Operation = Operation.Restart
            });
        }

        public async Task RestartAsync(string id)
        {
            await ControlAsync(new ControlRequest
            {
                Id = id,
                Operation = Operation.Restart
            });
        }

        public void Clear(string id)
        {
            Control(new ControlRequest
            {
                Id = id,
                Operation = Operation.Clear
            });
        }

        public async Task ClearAsync(string id)
        {
            await ControlAsync(new ControlRequest
            {
                Id = id,
                Operation = Operation.Clear
            });
        }

        public Stream Backup(string id)
        {
            return Backup(new BackupRequest
            {
                Id = id
            });
        }

        public Stream Backup(BackupRequest request)
        {
            return client.Post(request);
        }

        public async Task<Stream> BackupAsync(string id)
        {
            return await BackupAsync(new BackupRequest
            {
                Id = id
            });
        }

        public async Task<Stream> BackupAsync(BackupRequest request)
        {
            return await client.PostAsync(request);
        }

        public DeploymentResponse Restore(string id, Stream fileStream)
        {
            return client.PostFile<DeploymentResponse>(
                $@"/deployment/{id}/Restore", 
                fileStream,  
                GetTimeStampDumpFileName(), 
                "application/octet-stream");
        }

        public async Task<DeploymentResponse> RestoreAsync(string id, Stream fileStream)
        {
            return await Task.FromResult(client.PostFile<DeploymentResponse>(
                $@"/deployment/{id}/Restore", 
                fileStream,  
                GetTimeStampDumpFileName(), 
                "application/octet-stream"));
        }

        public void Configure(string id, string configFile, string key, string value)
        {
            Control(new ControlRequest
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
            await ControlAsync(new ControlRequest
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

        public void Control(ControlRequest request)
        {
            client.Post(request);
        }

        public async Task ControlAsync(ControlRequest request)
        {
            await client.PostAsync(request);
        }

        private static string GetTimeStampDumpFileName() => $"{DateTime.UtcNow:yyyyMMddHHmmss}.dump";
    }
}
