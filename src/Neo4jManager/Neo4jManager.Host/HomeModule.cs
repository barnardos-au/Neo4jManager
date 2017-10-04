using System.Collections.Generic;
using AutoMapper;
using Nancy;
using Neo4jManager.Host.Deployments;

namespace Neo4jManager.Host
{
    public class HomeModule : NancyModule
    {
        public HomeModule(INeo4jDeploymentsPool pool, IMapper mapper) : base("")
        {
            // Get all deployments
            Get("/", _ =>
            {
                var viewModel = new DeploymentsViewModel
                {
                    Deployments = mapper.Map<IEnumerable<Deployment>>(pool.Deployments)
                };

                return Negotiate
                    .WithModel(viewModel)
                    .WithView("Home");
            });
        }
    }
}