using System.Linq;
using Neo4jManager.ServiceModel;
using ServiceStack;

namespace Neo4jManager.ServiceInterface
{
    public class DeploymentsService : Service
    {
        private readonly INeo4jDeploymentsPool pool;

        public DeploymentsService(INeo4jDeploymentsPool pool)
        {
            this.pool = pool;
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
