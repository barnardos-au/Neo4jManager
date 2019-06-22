using System.IO;
using ServiceStack.Configuration;

namespace Neo4jManager
{
    public static class AppSettingsExtensions
    {
        public static string DeploymentsBasePath(this IAppSettings appSettings)
        {
            return Path.Combine(appSettings.GetString(AppSettingsKeys.Neo4jBasePath), "deployments");
        }
    }
}