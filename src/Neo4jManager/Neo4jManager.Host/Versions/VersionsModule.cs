using System.Linq;
using Nancy;

namespace Neo4jManager.Host.Versions
{
    public class VersionsModule : NancyModule
    {
        public VersionsModule(INeo4jVersionRepository versionRepository) : base("/versions")
        {
            // Get all versions
            Get("/", _ =>
            {
                var viewModel = new VersionsViewModel
                {
                    Versions = versionRepository.GetVersions()
                };

                return Negotiate
                    .WithModel(viewModel)
                    .WithView("Versions");
            });

            // Get single version by number
            Get("/{VersionNumber}", ctx =>
            {
                string versionNumber = ctx.VersionNumber.ToString();
                return versionRepository.GetVersions().Single(v => v.VersionNumber == versionNumber);
            });
        }
    }
}
