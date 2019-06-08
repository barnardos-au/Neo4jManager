using ServiceStack;
using System.Linq;
using Neo4jManager.ServiceModel;

namespace Neo4jManager.ServiceInterface
{
    // ReSharper disable once InconsistentNaming
    public class DeploymentService : Service
    {
        private readonly INeo4jDeploymentsPool pool;

        public DeploymentService(INeo4jDeploymentsPool pool)
        {
            this.pool = pool;
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
        
        // Get All
        public DeploymentsResponse Get(DeploymentsRequest request)
        {
            var response = new DeploymentsResponse
            {
                Deployments = pool.Select(kvp => kvp.ConvertTo<Deployment>())
            };

            return response;
        }
        
        // Delete All
        public DeploymentsResponse Delete(DeploymentsRequest request)
        {
            pool.DeleteAll();
            Helper.KillJavaProcesses();

            return new DeploymentsResponse();
        }
    }
}
