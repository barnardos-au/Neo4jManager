using System;
using System.Collections.Generic;
using Funq;
using Neo4jManager.ServiceInterface;
using Neo4jManager.ServiceModel;
using NUnit.Framework;
using ServiceStack;
using ServiceStack.Configuration;
using Version = Neo4jManager.ServiceModel.Version;

namespace Neo4jManager.Tests
{
    public class IntegrationTest
    {
        const string BaseUri = "http://localhost:2000/";
        private readonly ServiceStackHost appHost;

        class AppHost : AppSelfHostBase
        {
            public AppHost() : base(nameof(IntegrationTest), typeof(DeploymentService).Assembly)
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
                container.RegisterAutoWiredAs<FileCopy, IFileCopy>();
                container.Register<INeo4jManagerConfig>(c => new Neo4jManagerConfig
                {
                    Neo4jBasePath = @"c:\Neo4jManager",
                    StartBoltPort = 7691,
                    StartHttpPort = 7401
                }).ReusedWithin(ReuseScope.None);
                container.RegisterAutoWiredAs<ZuluJavaResolver, IJavaResolver>().ReusedWithin(ReuseScope.None);
                container.RegisterAutoWiredAs<Neo4jInstanceFactory, INeo4jInstanceFactory>().ReusedWithin(ReuseScope.None);
                container.RegisterAutoWiredAs<Neo4jDeploymentsPool, INeo4jDeploymentsPool>().ReusedWithin(ReuseScope.Container);
                
                Plugins.Add(new CancellableRequestsFeature());
            }
        }

        public IntegrationTest()
        {
            appHost = new AppHost()
                .Init()
                .Start(BaseUri);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown() => appHost.Dispose();

        public IServiceClient CreateClient() => new JsonServiceClient(BaseUri);

        [Test]
        public void Can_Install_And_Start_Neo4j()
        {
            var client = CreateClient();

            try
            {
                var deploymentResponse = client.Post(new DeploymentRequest
                {
                    Id = "1",
                    Version = "3.5.3"
                });
                var controlResponse = client.Post(new ControlRequest
                {
                    Id = "1",
                    Operation = Operation.Start
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }
    }
}