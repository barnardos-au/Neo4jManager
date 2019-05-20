using System.Collections.Generic;
using ServiceStack;

namespace Neo4jManager.ServiceModel.Deployments
{
    [Route("/deployment", "POST")]
    [Route("/deployment/{Id}", "GET,DELETE")]
    public class DeploymentRequest : IReturn<DeploymentResponse>
    {
        public string Id { get; set; }
        public string Version { get; set; }
        
        public List<Setting> Settings { get; set; }
        public List<string> PluginUrls { get; set; }
    }
}
