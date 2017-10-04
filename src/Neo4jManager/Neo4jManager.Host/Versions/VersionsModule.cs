using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Nancy;

namespace Neo4jManager.Host.Versions
{
    public class VersionsModule : NancyModule
    {
        public VersionsModule(IMapper mapper) : base("/versions")
        {
            // Get all versions
            Get("/", _ => mapper.Map<IEnumerable<Version>>(Neo4jVersions.GetVersions()));

            // Get single version by number
            Get("/{VersionNumber}", ctx =>
            {
                string versionNumber = ctx.VersionNumber.ToString();
                return mapper.Map<Version>(Neo4jVersions.GetVersions().Single(v => v.Version == versionNumber));
            });

        }
    }
}
