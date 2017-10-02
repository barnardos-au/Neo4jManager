using Nancy;

namespace Neo4jManager.Host.Versions
{
    public class VersionsModule : NancyModule
    {
        public VersionsModule(IFileCopy fileCopy) : base("/versions")
        {
            Get("/", _ => Neo4jVersions.GetVersions().AsVersionInfos());
        }
    }
}
