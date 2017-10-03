using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nancy.Owin;
using Neo4jManager.Host.Versions;

namespace Neo4jManager.Host
{
    public class Startup
    {
        public IServiceCollection Services { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IFileCopy, FileCopy>();

            Services = services;
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseOwin().UseNancy(o => o.Bootstrapper = new AutofacBootstrapper(Services));

            Mapper.Initialize(cfg => {
                cfg.CreateMap<Neo4jVersion, VersionInfo>();
            });
        }
    }
}
