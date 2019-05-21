using System;
using System.Net;
using System.Threading.Tasks;
using Neo4jManager.ServiceModel;
using ServiceStack;
using ServiceStack.Web;

namespace Neo4jManager.ServiceInterface
{
    public class ControlService : Service
    {
        private readonly INeo4jDeploymentsPool pool;

        public ControlService(INeo4jDeploymentsPool pool)
        {
            this.pool = pool;
        }

        public async Task<DeploymentResponse> Post(ControlRequest request)
        {
            var instance = pool[request.Id];
            
            using (var cancellableRequest = Request.CreateCancellableRequest())
            {
                switch (request.Operation)
                {
                    case Operation.Start:
                        await pool[request.Id].Start(cancellableRequest.Token);
                        break;

                    case Operation.Stop:
                        await pool[request.Id].Stop(cancellableRequest.Token);
                        break;

                    case Operation.Restart:
                        await pool[request.Id].Restart(cancellableRequest.Token);
                        break;
                    
                    case Operation.Clear:
                        await pool[request.Id].Clear(cancellableRequest.Token);
                        break;
                    
                    case Operation.Backup:
                        await pool[request.Id].Backup(
                            cancellableRequest.Token,
                            request.DestinationPath,
                            request.StopInstanceBeforeBackup);
                        break;

                    case Operation.Restore:
                        await pool[request.Id].Restore(
                            cancellableRequest.Token,
                            request.SourcePath);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return new DeploymentResponse
            {
                Deployment = instance.ConvertTo<Deployment>()
            };
        }
    }
}
