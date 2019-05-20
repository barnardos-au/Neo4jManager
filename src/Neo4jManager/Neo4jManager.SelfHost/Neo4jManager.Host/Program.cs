using AutoMapper;
using Funq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Neo4jManager.ServiceInterface;
using Neo4jManager.ServiceModel.Deployments;
using ServiceStack;
using ServiceStack.Api.Swagger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Neo4jManager.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseUrls(Environment.GetEnvironmentVariable("Neo4jManager.Url") ?? "http://localhost:7400/")
                .Build();

            host.Run();
        }
    }

    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseServiceStack(new AppHost(env));

            app.Run(context =>
            {
                context.Response.Redirect("/metadata");
                return Task.FromResult(0);
            });
        }
    }

    public class AppHost : AppHostBase
    {
        private readonly IHostingEnvironment hostingEnvironment;

        public AppHost(IHostingEnvironment hostingEnvironment) : base("Neo4jManager", typeof(DeploymentService).Assembly)
        {
            this.hostingEnvironment = hostingEnvironment;
        }

        public override void Configure(Container container)
        {
            container.RegisterAutoWiredAs<FileCopy, IFileCopy>();
            container.Register<INeo4jManagerConfig>(c => new Neo4jManagerConfig
            {
                Neo4jBasePath = @"c:\Neo4jManager",
                StartBoltPort = 7687,
                StartHttpPort = 7401
            }).ReusedWithin(ReuseScope.None);
            container.RegisterAutoWiredAs<Neo4jInstanceFactory, INeo4jInstanceFactory>().ReusedWithin(ReuseScope.None);
            container.RegisterAutoWiredAs<Neo4jDeploymentsPool, INeo4jDeploymentsPool>();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Neo4jVersion, ServiceModel.Versions.Version>()
                    .ForMember(dest => dest.VersionNumber, opts => opts.MapFrom(s => s.Version));
                cfg.CreateMap<Neo4jEndpoints, Endpoints>();
                cfg.CreateMap<KeyValuePair<string, INeo4jInstance>, Deployment>()
                    .ForMember(dest => dest.Id, opts => opts.MapFrom(s => s.Key))
                    .ForMember(dest => dest.DataPath, opts => opts.MapFrom(s => s.Value.DataPath))
                    .ForMember(dest => dest.Version, opts => opts.MapFrom(s => s.Value.Version))
                    .ForMember(dest => dest.Endpoints, opts => opts.MapFrom(s => s.Value.Endpoints))
                    .ForMember(dest => dest.Status, opts => opts.MapFrom(s => s.Value.Status.GetDescription()));
            });
            container.Register(c => mapperConfig.CreateMapper()).ReusedWithin(ReuseScope.None);
            container.Register<INeo4jVersionRepository>(c => new Neo4jVersionRepository(Path.Combine(hostingEnvironment.ContentRootPath, "versions.json"))).ReusedWithin(ReuseScope.None);

            ConfigurePlugins();
        }

        private void ConfigurePlugins()
        {
            Plugins.Add(new CancellableRequestsFeature());
            Plugins.Add(new SwaggerFeature());
            Plugins.Add(new CorsFeature());
        }
    }
}