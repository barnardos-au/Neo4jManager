using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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
using ServiceStack.Logging;
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
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
        {
            appLifetime.ApplicationStopped.Register(OnStopped);
            
            app.UseServiceStack(new AppHost(env));

            app.Run(context =>
            {
                context.Response.Redirect("/metadata");
                return Task.FromResult(0);
            });
        }

        private void OnStopped()
        {
            Neo4jManager.ServiceInterface.Helper.KillJavaProcesses();
        }
    }
    
    public class AppHost : AppHostBase
    {
        public AppHost(IHostingEnvironment hostingEnvironment) : base("Neo4jManager", typeof(DeploymentService).Assembly)
        {
            var versions = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "versions.json"))
                .FromJson<IEnumerable<Version>>()
                .ToJsv();

            AppSettings = new MultiAppSettingsBuilder()
                .AddEnvironmentalVariables()
                .AddTextFile(Path.Combine(AppContext.BaseDirectory, "app.settings"))
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
            builder.RegisterType<WatchdogService>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<ZuluJavaResolver>().AsImplementedInterfaces();
            builder.RegisterType<Neo4jInstanceFactory>().AsImplementedInterfaces();
            builder.RegisterType<Neo4jV3JavaInstanceProvider>().AsImplementedInterfaces().AsSelf();
            builder.RegisterType<Neo4jDeploymentsPool>().AsImplementedInterfaces().SingleInstance();
            builder.Register(ctx => AppSettings).AsImplementedInterfaces().SingleInstance();
            builder.Register(ctx => LogManager.LogFactory.GetLogger(typeof(IService))).AsImplementedInterfaces();
            
            IContainerAdapter adapter = new AutofacIocAdapter(builder.Build());
            container.Adapter = adapter;

            ConfigurePlugins();
            ConfigureLogger();
            
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

        private void ConfigureLogger()
        {
            LogManager.LogFactory = new ConsoleLogFactory(debugEnabled:true); 
        }

        public override void OnAfterInit()
        {
            base.OnAfterInit();

            var logger = Container.Resolve<ILog>();
            var appSettings = Container.Resolve<IAppSettings>();
            
            try
            {
                var basePath = appSettings.DeploymentsBasePath();
                if (Directory.Exists(basePath))
                {
                    logger.Debug($"Clearing folder {basePath}");
                    Directory.Delete(basePath, true);
                }
            }
            catch (Exception e)
            {
                logger.Error(e);
            }
            
            var watchdog = Container.Resolve<IWatchdogService>();

            watchdog.StartAsync(CancellationToken.None).Wait();
        }
        
        protected override void Dispose(bool disposing)
        {
            var watchdog = Container.Resolve<IWatchdogService>();
            watchdog.StopAsync(CancellationToken.None).Wait();

            var pool = Container.Resolve<INeo4jDeploymentsPool>();
            pool.DeleteAll(true);
        }
    }
}
