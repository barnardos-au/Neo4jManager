using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Neo4jManager.ServiceModel;

namespace Neo4jManager.Client
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public interface INeo4jManagerClient
    {
        IEnumerable<Version> GetVersions();
        Task<IEnumerable<Version>> GetVersionsAsync();
        IEnumerable<Deployment> GetDeployments();
        Task<IEnumerable<Deployment>> GetDeploymentsAsync();
        Deployment GetDeployment(string id);
        Task<Deployment> GetDeploymentAsync(string id);
        void DeleteAll();
        Task DeleteAllAsync();
        Deployment Create(string versionNumber, List<Setting> settings = null,
            List<string> pluginUrls = null);
        Task<Deployment> CreateAsync(string versionNumber, List<Setting> settings = null,
            List<string> pluginUrls = null);
        void Delete(string id);
        Task DeleteAsync(string id);
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
    }
}
