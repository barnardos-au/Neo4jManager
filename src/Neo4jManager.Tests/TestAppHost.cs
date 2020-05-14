using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Autofac;
using Funq;
using Neo4jManager.ServiceInterface;
using Neo4jManager.V3;
using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.Logging;
using Version = Neo4jManager.ServiceModel.Version;

namespace Neo4jManager.Tests
{
    public class TestAppHost : AppSelfHostBase
    {
        public TestAppHost() : base(nameof(IntegrationTest), typeof(DeploymentService).Assembly)
        {
            var versionsJsv = new List<Version>
            {
                new Version
                {
                    VersionNumber = "3.5.3",
                    DownloadUrl = "https://neo4j.com/artifact.php?name=neo4j-community-3.5.3-windows.zip",
                    ZipFileName = "neo4j-community-3.5.3-windows.zip",
                    Architecture = "V3"
                }
            }.ToJsv();
            
            AppSettings = new MultiAppSettingsBuilder()
                .AddDictionarySettings(new Dictionary<string, string>
                {
                    { AppSettingsKeys.Versions, versionsJsv },
                    { AppSettingsKeys.Neo4jBasePath, @"c:\Neo4jManager" },
                    { AppSettingsKeys.StartBoltPort, "7691" },
                    { AppSettingsKeys.StartHttpPort, "7401" },
                    { AppSettingsKeys.WatchDogInterval, "00:01:00" },
                    { AppSettingsKeys.DefaultLeasePeriod, "00:05:00" },
                    { AppSettingsKeys.ExpiryPeriod, "00:05:00" },
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
            
            LogManager.LogFactory = new ConsoleLogFactory(); 

            Plugins.Add(new CancellableRequestsFeature());
        }

        public override void OnAfterInit()
        {
            base.OnAfterInit();
            
            var watchdog = Container.Resolve<IWatchdogService>();

            watchdog.StartAsync(CancellationToken.None).Wait();
        }

        protected override void Dispose(bool disposing)
        {
            var pool = Container.Resolve<INeo4jDeploymentsPool>();
            pool.DeleteAll(true);
        }
    }
}
