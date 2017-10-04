using Nancy;

namespace Neo4jManager.Host.Versions
{
    public class VersionsModule : NancyModule
    {
        public VersionsModule() : base("/versions")
        {
            Get("/", _ => Neo4jVersions.GetVersions().AsVersionInfos());
        }
    }
}
