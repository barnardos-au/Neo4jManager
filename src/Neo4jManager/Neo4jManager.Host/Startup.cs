using System.Collections.Generic;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nancy.Owin;
using Neo4jManager.Host.Deployments;
using Neo4jManager.Host.Versions;

namespace Neo4jManager.Host
{
    public class Startup
    {
        public IServiceCollection Services { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IFileCopy, FileCopy>();
            services.AddTransient<INeo4jManagerConfig>(provider => new Neo4jManagerConfig
            {
                Neo4jBasePath = @"c:\Neo4jManager",
                StartBoltPort = 7687,
                StartHttpPort = 7401
            });
            services.AddTransient<INeo4jInstanceFactory, Neo4jInstanceFactory>();
            services.AddSingleton<INeo4jDeploymentsPool, Neo4jDeploymentsPool>();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Neo4jVersion, Version>()
                    .ForMember(dest => dest.VersionNumber, opts => opts.MapFrom(s => s.Version));
                cfg.CreateMap<Neo4jEndpoints, Endpoints>();
                cfg.CreateMap<KeyValuePair<string, INeo4jInstance>, Deployment>()
                    .ForMember(dest => dest.Id, opts => opts.MapFrom(s => s.Key))
                    .ForMember(dest => dest.DataPath, opts => opts.MapFrom(s => s.Value.DataPath))
                    .ForMember(dest => dest.Version, opts => opts.MapFrom(s => s.Value.Version))
                    .ForMember(dest => dest.Endpoints, opts => opts.MapFrom(s => s.Value.Endpoints))
                    .ForMember(dest => dest.Status, opts => opts.MapFrom(s => s.Value.Status.GetDescription()));
            });
            services.AddTransient(provider => mapperConfig.CreateMapper());

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
        }
    }
}
