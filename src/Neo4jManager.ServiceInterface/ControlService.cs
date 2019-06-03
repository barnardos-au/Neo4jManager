using System;
using System.Linq;
using System.Threading.Tasks;
using Neo4jManager.ServiceModel;
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

        public async Task<DeploymentResponse> Post(ControlRequest request)
        {
            if (!pool.ContainsKey(request.Id))
                throw HttpError.NotFound($"Deployment {request.Id} not found");

            var keyedInstance = pool.Single(p => p.Key == request.Id);
            
            using (var cancellableRequest = Request.CreateCancellableRequest())
            {
                switch (request.Operation)
                {
                    case Operation.Start:
                        await keyedInstance.Value.Start(cancellableRequest.Token);
                        break;

                    case Operation.Stop:
                        await keyedInstance.Value.Stop(cancellableRequest.Token);
                        break;

                    case Operation.Restart:
                        await keyedInstance.Value.Restart(cancellableRequest.Token);
                        break;
                    
                    case Operation.Clear:
                        await keyedInstance.Value.Clear(cancellableRequest.Token);
                        break;
                    
                    case Operation.Backup:
                        await keyedInstance.Value.Backup(
                            cancellableRequest.Token);
                        break;

                    case Operation.Restore:
                        await keyedInstance.Value.Restore(
                            cancellableRequest.Token,
                            request.SourcePath);
                        break;

                    case Operation.Configure:
                        keyedInstance.Value.Configure(
                            request.Setting.ConfigFile,
                            request.Setting.Key,
                            request.Setting.Value);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return keyedInstance.ConvertTo<DeploymentResponse>();
        }
    }
}
