using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Neo4jManager.ServiceModel;
using Version = Neo4jManager.ServiceModel.Version;

namespace Neo4jManager.Client
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public interface INeo4jManagerClient
    {
        IEnumerable<Version> GetVersions();
        IEnumerable<Version> GetVersions(VersionsRequest request);
        Task<IEnumerable<Version>> GetVersionsAsync();
        Task<IEnumerable<Version>> GetVersionsAsync(VersionsRequest request);

        IEnumerable<Deployment> GetDeployments();
        IEnumerable<Deployment> GetDeployments(DeploymentsRequest request);
        Task<IEnumerable<Deployment>> GetDeploymentsAsync();
        Task<IEnumerable<Deployment>> GetDeploymentsAsync(DeploymentsRequest request);

        Deployment GetDeployment(string id);
        Deployment GetDeployment(DeploymentRequest request);
        Task<Deployment> GetDeploymentAsync(string id);
        Task<Deployment> GetDeploymentAsync(DeploymentRequest request);

        void DeleteAll(bool permanent = false);
        void DeleteAll(DeploymentsRequest request);
        Task DeleteAllAsync(bool permanent = false);
        Task DeleteAllAsync(DeploymentsRequest request);

        Deployment Create(
            string versionNumber, 
            TimeSpan? leasePeriod = null,
            List<Setting> settings = null,
            List<string> pluginUrls = null,
            string restoreDumpFile = null,
            bool autoStart = false);
        Deployment Create(CreateDeploymentRequest request);
        Task<Deployment> CreateAsync(
            string versionNumber, 
            TimeSpan? leasePeriod = null,
            List<Setting> settings = null,
            List<string> pluginUrls = null,
            string restoreDumpFile = null,
            bool autoStart = false);
        Task<Deployment> CreateAsync(CreateDeploymentRequest request);
        
        void Delete(string id, bool permanent = false);
        void Delete(DeploymentRequest request);
        Task DeleteAsync(string id, bool permanent = false);
        Task DeleteAsync(DeploymentRequest request);

        void Start(string id);
        Task StartAsync(string id);

        void Stop(string id);
        Task StopAsync(string id);

        void Restart(string id);
        Task RestartAsync(string id);

        void Clear(string id);
        Task ClearAsync(string id);
        
        Stream Backup(string id);
        Task<Stream> BackupAsync(string id);
        
        DeploymentResponse Restore(string id, Stream fileStream);
        Task<DeploymentResponse> RestoreAsync(string id, Stream fileStream);
		
        void Configure(string id, string configFile, string key, string value);
		Task ConfigureAsync(string id, string configFile, string key, string value);
        
        void Control(ControlRequest request);
        Task ControlAsync(ControlRequest request);
    }
}
