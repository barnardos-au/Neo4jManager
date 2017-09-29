using ServiceStack;

namespace Neo4jManager.Host
{
    public class HelloService : Service
    {
        public object Any(Hello request)
        {
            return new HelloResponse { Result = $"Hello, {request.Name}!" };
        }
    }

}
