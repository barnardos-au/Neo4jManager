using ServiceStack;

namespace Neo4jManager.ServiceModel.Deployments
{
    [Route("/deployment/{Id}/configure")]
    public class ConfigureRequest : IReturn<DeploymentResponse>
    {
        public string Id { get; set; }
        public string ConfigFile { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
