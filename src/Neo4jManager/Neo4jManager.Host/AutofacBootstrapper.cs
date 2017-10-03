using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Nancy.Bootstrappers.Autofac;

namespace Neo4jManager.Host
{
    public class AutofacBootstrapper : AutofacNancyBootstrapper
    {
        private readonly IServiceCollection _serviceCollection;

        public AutofacBootstrapper(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        protected override void ConfigureApplicationContainer(ILifetimeScope existingContainer)
        {
            base.ConfigureApplicationContainer(existingContainer);

            existingContainer.Update(builder =>
            {
                builder.Populate(_serviceCollection);
            });
        }
    }
}
