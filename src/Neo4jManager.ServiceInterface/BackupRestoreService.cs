using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Neo4jManager.ServiceModel;
using ServiceStack;

namespace Neo4jManager.ServiceInterface
{
    public class BackupRestoreService : Service
    {
        private readonly INeo4jDeploymentsPool pool;

        public BackupRestoreService(INeo4jDeploymentsPool pool)
        {
            this.pool = pool;
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
        public async Task Post(RestoreRequest request)
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

            file.SaveTo(restorePath);

            using (var cancellableRequest = Request.CreateCancellableRequest())
            {
                await keyedInstance.Value.Restore(
                    cancellableRequest.Token,
                    restorePath);
            }
        }
    }
}
