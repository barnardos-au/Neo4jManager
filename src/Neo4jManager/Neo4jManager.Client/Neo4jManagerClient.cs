using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using ServiceStack;

namespace Neo4jManager.Client
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Neo4jManagerClient : INeo4jManagerClient
    {
        private readonly string baseUrl;

        public Neo4jManagerClient(int port)
        {
            baseUrl = $"http://localhost:{port}";
        }

        public IEnumerable<Version> GetVersions()
        {
            return $"{baseUrl}/versions"
                .GetJsonFromUrl()
                .FromJson<VersionList>()
                .Versions;
        }

        public async Task<IEnumerable<Version>> GetVersionsAsync()
        {
            var response = await $"{baseUrl}/versions"
                .GetStringFromUrlAsync();

            return response
                .FromJson<VersionList>()
                .Versions;
        }

        public IEnumerable<Deployment> GetDeployments()
        {
            return $"{baseUrl}/deployments"
                .GetJsonFromUrl()
                .FromJson<DeploymentList>()
                .Deployments;
        }

        public async Task<IEnumerable<Deployment>> GetDeploymentsAsync()
        {
            var response = await $"{baseUrl}/deployments"
                .GetStringFromUrlAsync();

            return response
                .FromJson<DeploymentList>()
                .Deployments;
        }

        public Deployment GetDeployment(string id)
        {
            return $"{baseUrl}/deployments/{id}"
                .GetJsonFromUrl()
                .FromJson<Deployment>();
        }

        public async Task<Deployment> GetDeploymentAsync(string id)
        {
            var response = await $"{baseUrl}/deployments/{id}"
                .GetStringFromUrlAsync();

            return response
                .FromJson<Deployment>();
        }

        public void DeleteAll()
        {
            $"{baseUrl}/deployments/all"
                .DeleteFromUrl();
        }

        public async Task DeleteAllAsync()
        {
            await $"{baseUrl}/deployments/all".DeleteFromUrlAsync();
        }

        public Deployment Create(string id, string versionNumber)
        {
            return $"{baseUrl}/deployments/create"
                .PostJsonToUrl(new CreateDeployment { Id = id, Version = versionNumber })
                .FromJson<Deployment>();
        }

        public async Task<Deployment> CreateAsync(string id, string versionNumber)
        {
            var response = await $"{baseUrl}/deployments/create"
                .PostJsonToUrlAsync(new CreateDeployment { Id = id, Version = versionNumber });

            return response
                .FromJson<Deployment>();
        }

        public void Delete(string id)
        {
            $"{baseUrl}/deployments/{id}"
                .DeleteFromUrl();
        }

        public async Task DeleteAsync(string id)
        {
            await $"{baseUrl}/deployments/{id}"
                .DeleteFromUrlAsync();
        }

        public void Start(string id)
        {
            $"{baseUrl}/deployments/{id}/start"
                .PostToUrl(null, requestFilter: req => req.ContentLength = 0);
        }

        public async Task StartAsync(string id)
        {
            await $"{baseUrl}/deployments/{id}/start"
                .PostJsonToUrlAsync(null, req => req.ContentLength = 0);
        }

        public void Stop(string id)
        {
            $"{baseUrl}/deployments/{id}/stop"
                .PostToUrl(null, requestFilter: req => req.ContentLength = 0);
        }

        public async Task StopAsync(string id)
        {
            await $"{baseUrl}/deployments/{id}/stop"
                .PostJsonToUrlAsync(null, req => req.ContentLength = 0);
        }

        public void Restart(string id)
        {
            $"{baseUrl}/deployments/{id}/restart"
                .PostToUrl(null, requestFilter: req => req.ContentLength = 0);
        }

        public async Task RestartAsync(string id)
        {
            await $"{baseUrl}/deployments/{id}/restart"
                .PostJsonToUrlAsync(null, req => req.ContentLength = 0);
        }

        public void Clear(string id)
        {
            $"{baseUrl}/deployments/{id}/clear"
                .PostToUrl(null, requestFilter: req => req.ContentLength = 0);
        }

        public async Task ClearAsync(string id)
        {
            await $"{baseUrl}/deployments/{id}/clear"
                .PostJsonToUrlAsync(null, req => req.ContentLength = 0);
        }

        public void Backup(string id, string destinationPath, bool stopInstanceBeforeBackup = true)
        {
            $"{baseUrl}/deployments/{id}/backup"
                .PostJsonToUrl(new Backup { DestinationPath = destinationPath, StopInstanceBeforeBackup = stopInstanceBeforeBackup });
        }

        public async Task BackupAsync(string id, string destinationPath, bool stopInstanceBeforeBackup = true)
        {
            await $"{baseUrl}/deployments/{id}/backup"
                .PostJsonToUrlAsync(new Backup { DestinationPath = destinationPath, StopInstanceBeforeBackup = stopInstanceBeforeBackup });
        }

        public void Restore(string id, string sourcePath)
        {
            $"{baseUrl}/deployments/{id}/restore"
                .PostJsonToUrl(new Restore { SourcePath = sourcePath });
        }

        public async Task RestoreAsync(string id, string sourcePath)
        {
            await $"{baseUrl}/deployments/{id}/restore"
                .PostJsonToUrlAsync(new Restore { SourcePath = sourcePath });
        }

        public void Configure(string id, string configFile, string key, string value)
        {
            $"{baseUrl}/deployments/{id}/config"
                .PostJsonToUrl(new Config { ConfigFile = configFile, Key = key, Value = value });
        }

        public async Task ConfigureAsync(string id, string configFile, string key, string value)
        {
            await $"{baseUrl}/deployments/{id}/config"
                .PostJsonToUrlAsync(new Config { ConfigFile = configFile, Key = key, Value = value });
        }

    }
}
