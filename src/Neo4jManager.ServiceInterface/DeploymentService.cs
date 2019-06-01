using ServiceStack;
using System;
using System.Linq;
using System.Threading.Tasks;
using Neo4jManager.ServiceModel;
using ServiceStack.Configuration;

namespace Neo4jManager.ServiceInterface
{
    // ReSharper disable once InconsistentNaming
    public class DeploymentService : Service
    {
        private readonly INeo4jDeploymentsPool pool;
        private readonly IAppSettings appSettings;

        public DeploymentService(
            INeo4jDeploymentsPool pool,
            IAppSettings appSettings)
        {
            this.pool = pool;
            this.appSettings = appSettings;
        }

        // Get by Id
        public DeploymentResponse Get(DeploymentRequest request)
        {
            if (!pool.ContainsKey(request.Id))
                throw HttpError.NotFound($"Deployment {request.Id} not found");
            
            var keyedInstance = pool.SingleOrDefault(p => p.Key == request.Id);
            
            return keyedInstance.ConvertTo<DeploymentResponse>();
        }

        // Delete
        public DeploymentResponse Delete(DeploymentRequest request)
        {
            if (!pool.ContainsKey(request.Id))
                throw HttpError.NotFound($"Deployment {request.Id} not found");

            var keyedInstance = pool.Single(p => p.Key == request.Id);
            
            pool.Delete(request.Id);

            return keyedInstance.ConvertTo<DeploymentResponse>();
        }
    }
}
