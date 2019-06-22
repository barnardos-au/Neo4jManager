using System.Collections.Generic;
using Autofac;
using Funq;
using Neo4jManager.ServiceInterface;
using Neo4jManager.ServiceModel;
using Neo4jManager.V3;
using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.Logging;

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
                    { AppSettingsKeys.Versions, versionsJsv }
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
            builder.Register(ctx => LogManager.LogFactory.GetLogger(typeof(IService))).AsImplementedInterfaces();
        
            IContainerAdapter adapter = new AutofacIocAdapter(builder.Build());
            container.Adapter = adapter;
            
            LogManager.LogFactory = new ConsoleLogFactory(); 

            Plugins.Add(new CancellableRequestsFeature());
        }

        protected override void Dispose(bool disposing)
        {
            var pool = Container.Resolve<INeo4jDeploymentsPool>();
            pool.DeleteAll(true);
        }
    }
}
