using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Funq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Neo4jManager.ServiceInterface;
using Neo4jManager.ServiceModel;
using Neo4jManager.V3;
using ServiceStack;
using ServiceStack.Api.OpenApi;
using ServiceStack.Configuration;
using Version = Neo4jManager.ServiceModel.Version;

namespace Neo4jManager.Host
{
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
        public AppHost(IHostingEnvironment hostingEnvironment) : base("Neo4jManager", typeof(DeploymentService).Assembly)
        {
            var versions = File.ReadAllText(Path.Combine(System.AppContext.BaseDirectory, "versions.json"))
                .FromJson<IEnumerable<Version>>()
                .ToJsv();

            AppSettings = new MultiAppSettingsBuilder()
                .AddEnvironmentalVariables()
                .AddDictionarySettings(new Dictionary<string, string>
                {
                    { AppSettingsKeys.Versions, versions }
                })
                .Build();
        }

        public override void Configure(Container container)
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<FileCopy>().AsImplementedInterfaces();
            builder.Register(ctx => new Neo4jManagerConfig
            {
                Neo4jBasePath = @"c:\Neo4jManager",
                StartBoltPort = 7691,
                StartHttpPort = 7401
            }).AsImplementedInterfaces();
            builder.RegisterType<ZuluJavaResolver>().AsImplementedInterfaces();
            builder.RegisterType<Neo4jInstanceFactory>().AsImplementedInterfaces();
            builder.RegisterType<Neo4jV3JavaInstanceProvider>().AsImplementedInterfaces().AsSelf();
            builder.RegisterType<Neo4jDeploymentsPool>().AsImplementedInterfaces().SingleInstance();
            
            IContainerAdapter adapter = new AutofacIocAdapter(builder.Build());
            container.Adapter = adapter;

            ConfigurePlugins();
            Neo4jManager.ServiceInterface.Helper.ConfigureMappers();
        }

        private void ConfigurePlugins()
        {
            Plugins.Add(new CancellableRequestsFeature());
            Plugins.Add(new OpenApiFeature
            {
                OperationFilter = (s, operation) =>
                {
                    if (operation.RequestType != typeof(BackupRequest).Name) return;

                    operation.Produces = new[] {"application/octet-stream"}.ToList();
                }
            });
            Plugins.Add(new CorsFeature());
        }
    }
}
