using System;
using System.Net;
using Neo4jManager.ServiceModel.Deployments;
using ServiceStack;

namespace Neo4jManager.ServiceInterface
{
    public class ControlService : Service
    {
        private readonly INeo4jDeploymentsPool pool;

        public ControlService(INeo4jDeploymentsPool pool)
        {
            this.pool = pool;
        }

        public object Post(ControlRequest request)
        {
            using (var cancellableRequest = Request.CreateCancellableRequest())
            {
                switch (request.Operation)
                {
                    case Operation.Start:
                        pool[request.Id].Start(cancellableRequest.Token);
                        break;

                    case Operation.Stop:
                        pool[request.Id].Stop(cancellableRequest.Token);
                        break;

                    case Operation.Restart:
                        pool[request.Id].Restart(cancellableRequest.Token);
                        break;
                    
                    case Operation.Clear:
                        pool[request.Id].Clear(cancellableRequest.Token);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return new HttpResult(HttpStatusCode.NoContent);
        }
    }
}
