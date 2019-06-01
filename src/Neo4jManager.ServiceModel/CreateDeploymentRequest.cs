using System;
using System.Collections.Generic;
using ServiceStack;

namespace Neo4jManager.ServiceModel
{
    [Route("/deployment", "POST")]
    public class CreateDeploymentRequest : IReturn<DeploymentResponse>
    {
        public string Version { get; set; }
        public bool AutoStart { get; set; }
        public TimeSpan? LeasePeriod { get; set; }
        public string RestoreDumpFile { get; set; }
        
        public List<Setting> Settings { get; set; }
        public List<string> PluginUrls { get; set; }
    }
}
