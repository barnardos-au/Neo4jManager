using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Neo4jManager.ServiceModel;
using ServiceStack;
using ServiceStack.Logging;

namespace Neo4jManager.ServiceInterface
{
    public class BackupRestoreService : Service
    {
        private readonly INeo4jDeploymentsPool pool;
        private readonly ILog log;

        public BackupRestoreService(INeo4jDeploymentsPool pool, ILog log)
        {
            this.pool = pool;
            this.log = log;
        }

        // Backup & download (dump file)
        public async Task<HttpResult> Post(BackupRequest request)
        {
            if (!pool.ContainsKey(request.Id))
                throw HttpError.NotFound($"Deployment {request.Id} not found");

            var keyedInstance = pool.Single(p => p.Key == request.Id);

            if (!request.ObtainLastBackup)
            {
                using (var cancellableRequest = Request.CreateCancellableRequest())
                {
                    await keyedInstance.Value.Backup(
                        cancellableRequest.Token);
                }
            }
            
            return new HttpResult(
                new FileInfo(keyedInstance.Value.Deployment.LastBackupFile),
                "application/octet-stream",
                true); 
        }
        
        // Upload & restore (dump file)
        public async Task<DeploymentResponse> Post(RestoreRequest request)
        {
            if (!pool.ContainsKey(request.Id))
                throw HttpError.NotFound($"Deployment {request.Id} not found");

            var keyedInstance = pool.Single(p => p.Key == request.Id);

            var file = Request.Files.FirstOrDefault();
            if (file == null)
                throw HttpError.NotFound($"Upload file not found");
            
            var restorePath = Path.Combine(
                keyedInstance.Value.Deployment.BackupPath, 
                file.FileName);

            var restoreBasePath = Path.GetDirectoryName(restorePath);
            if (!Directory.Exists(restoreBasePath))
            {
                log.Debug($"Creating folder {restoreBasePath}");
                try
                {
                    Directory.CreateDirectory(restoreBasePath);
                }
                catch (Exception e)
                {
                    log.Error(e);
                }
            }

            file.SaveTo(restorePath);

            using (var cancellableRequest = Request.CreateCancellableRequest())
            {
                await keyedInstance.Value.Restore(
                    cancellableRequest.Token,
                    restorePath);
            }
            
            return keyedInstance.ConvertTo<DeploymentResponse>();
        }
    }
}
