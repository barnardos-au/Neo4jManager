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

        class TestAppHost : AppSelfHostBase
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
            appHost = new TestAppHost()
                .Init()
                .Start(BaseUri);
            
            Neo4jManager.ServiceInterface.Helper.ConfigureMappers();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown() => appHost.Dispose();

        public IServiceClient CreateClient() => new JsonServiceClient(BaseUri);

        [Test]
        public void Can_Install_And_Start_Neo4j()
        {
            var client = CreateClient();

            var deploymentResponse = client.Post(new DeploymentRequest
            {
                Id = "1",
                Version = "3.5.3"
            });

            Assert.IsNotNull(deploymentResponse.Deployment);
            
            var deployment = deploymentResponse.Deployment;
            Assert.IsNotNull(deployment.DataPath);
            Assert.IsTrue(deployment.DataPath.StartsWith(@"c:\neo4jmanager", StringComparison.OrdinalIgnoreCase));
            Assert.AreEqual("1", deployment.Id);
            Assert.IsNotNull(deployment.Version);
            Assert.AreEqual("V3", deployment.Version.Architecture);
            Assert.IsNotNull(deployment.Version.DownloadUrl);
            Assert.AreEqual("3.5.3", deployment.Version.VersionNumber);
            Assert.IsNotNull(deployment.Version.ZipFileName);
            Assert.IsNotNull(deployment.Endpoints);
            Assert.IsNotNull(deployment.Endpoints.BoltEndpoint);
            Assert.IsNotNull(deployment.Endpoints.HttpEndpoint);
            Assert.IsNull(deployment.Endpoints.HttpsEndpoint);
            Assert.AreEqual("Stopped", deployment.Status);

            var controlResponse = client.Post(new ControlRequest
            {
                Id = "1",
                Operation = Operation.Start
            });

            Assert.IsNotNull(controlResponse.Deployment);
            deployment = controlResponse.Deployment;
            Assert.IsNotNull(deployment.DataPath);
            Assert.IsTrue(deployment.DataPath.StartsWith(@"c:\neo4jmanager", StringComparison.OrdinalIgnoreCase));
            Assert.AreEqual("1", deployment.Id);
            Assert.IsNotNull(deployment.Version);
            Assert.AreEqual("V3", deployment.Version.Architecture);
            Assert.IsNotNull(deployment.Version.DownloadUrl);
            Assert.AreEqual("3.5.3", deployment.Version.VersionNumber);
            Assert.IsNotNull(deployment.Version.ZipFileName);
            Assert.IsNotNull(deployment.Endpoints);
            Assert.IsNotNull(deployment.Endpoints.BoltEndpoint);
            Assert.IsNotNull(deployment.Endpoints.HttpEndpoint);
            Assert.IsNull(deployment.Endpoints.HttpsEndpoint);
            Assert.AreEqual("Started", deployment.Status);

            controlResponse = client.Post(new ControlRequest
            {
                Id = "1",
                Operation = Operation.Stop
            });

            Assert.IsNotNull(controlResponse.Deployment);
            deployment = controlResponse.Deployment;
            Assert.AreEqual("Stopped", deployment.Status);
        }
    }
}