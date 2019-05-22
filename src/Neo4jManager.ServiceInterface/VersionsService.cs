using Neo4jManager.ServiceModel;
using ServiceStack;
using ServiceStack.Configuration;

namespace Neo4jManager.ServiceInterface
{
    public class VersionsService : Service
    {
        private readonly IAppSettings appSettings;

        public VersionsService(IAppSettings appSettings)
        {
            this.appSettings = appSettings;
        }

        // Get All
        public VersionsResponse Get(VersionsRequest request)
        {
            var response = new VersionsResponse
            {
                Versions = appSettings.Neo4jVersions()
            };

            return response;
        }
    }
}
