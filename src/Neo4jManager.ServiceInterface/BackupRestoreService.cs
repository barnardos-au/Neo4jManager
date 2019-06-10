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
            var instance = keyedInstance.Value;

            if (!Directory.Exists(instance.Deployment.BackupPath))
            {
                log.Debug($"Creating folder {instance.Deployment.BackupPath}");
                try
                {
                    Directory.CreateDirectory(instance.Deployment.BackupPath);
                }
                catch (Exception e)
                {
                    log.Error(e);
                }
            }
            
            string restorePath;
            
            if (string.IsNullOrEmpty(request.RestoreUrl))
            {
                var file = Request.Files.FirstOrDefault();
                if (file == null)
                    throw HttpError.NotFound($"Upload file not found");
                
                restorePath = Path.Combine(
                    keyedInstance.Value.Deployment.BackupPath, 
                    file.FileName);

                file.SaveTo(restorePath);
            }
            else
            {
                restorePath = Path.Combine(
                    keyedInstance.Value.Deployment.BackupPath,
                    Neo4jManager.Helper.GetTimeStampDumpFileName());

                var bytes = await request.RestoreUrl.GetBytesFromUrlAsync();
                File.WriteAllBytes(restorePath, bytes);
            }

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
