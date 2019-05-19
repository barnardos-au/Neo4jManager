using System.Linq;
using System.Net;
using Neo4jManager.ServiceModel.Deployments;
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

        public object Delete(DeploymentsRequest request)
        {
            pool.DeleteAll();
            Helper.KillNeo4jServices();
            HostHelper.KillJavaProcesses();

            return new HttpResult(HttpStatusCode.NoContent);
        }
    }
}
