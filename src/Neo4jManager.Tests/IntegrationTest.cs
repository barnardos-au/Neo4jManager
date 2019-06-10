using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Autofac;
using Funq;
using Neo4jManager.ServiceInterface;
using Neo4jManager.ServiceModel;
using Neo4jManager.V3;
using NUnit.Framework;
using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.Logging;
using Version = Neo4jManager.ServiceModel.Version;

namespace Neo4jManager.Tests
{
    public class IntegrationTest
    {
        const string BaseUri = "http://localhost:2000/";
        private readonly ServiceStackHost appHost;
        private IServiceClient client;

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
                
                LogManager.LogFactory = new ConsoleLogFactory(debugEnabled:true); 

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
        public void OneTimeTearDown()
        {
            appHost.Dispose();
        }

        [SetUp]
        public void Setup()
        {
            client = new JsonServiceClient(BaseUri);
        }

        [Test]
        public async Task Can_Install_And_Start()
        {
            var deploymentResponse = await client.PostAsync(new CreateDeploymentRequest
            {
                Version = "3.5.3",
                LeasePeriod = TimeSpan.FromMinutes(10)
            });

            Assert.IsNotNull(deploymentResponse);

            var deployment = deploymentResponse.Deployment;
            Assert.IsNotNull(deployment);
            Assert.IsNotNull(deployment.DataPath);
            Assert.IsTrue(deployment.DataPath.StartsWith(@"c:\neo4jmanager", StringComparison.OrdinalIgnoreCase));
            Assert.IsNotEmpty(deployment.Id);
            Assert.IsNotNull(deployment.Version);
            Assert.AreEqual("3.5.3", deployment.Version.VersionNumber);
            Assert.IsNotNull(deployment.Endpoints.BoltEndpoint);
            Assert.IsNotNull(deployment.Endpoints.HttpEndpoint);
            Assert.IsNull(deployment.Endpoints.HttpsEndpoint);
            Assert.AreEqual("Stopped", deployment.Status);

            var controlResponse = await client.PostAsync(new ControlRequest
            {
                Id = deployment.Id,
                Operation = Operation.Start
            });

            Assert.IsNotNull(controlResponse);
            
            deployment = controlResponse.Deployment;
            Assert.IsNotNull(deployment);
            Assert.IsNotNull(deployment.DataPath);
            Assert.IsTrue(deployment.DataPath.StartsWith(@"c:\neo4jmanager", StringComparison.OrdinalIgnoreCase));
            Assert.IsNotEmpty(deployment.Id);
            Assert.IsNotNull(deployment.Version);
            Assert.AreEqual("3.5.3", deployment.Version.VersionNumber);
            Assert.IsNotNull(deployment.Endpoints.BoltEndpoint);
            Assert.IsNotNull(deployment.Endpoints.HttpEndpoint);
            Assert.IsNull(deployment.Endpoints.HttpsEndpoint);
            Assert.AreEqual("Started", deployment.Status);
            
            controlResponse = await client.PostAsync(new ControlRequest
            {
                Id = deployment.Id,
                Operation = Operation.Stop
            });

            Assert.IsNotNull(controlResponse);

            deployment = controlResponse.Deployment;
            Assert.IsNotNull(deployment);
            Assert.AreEqual("Stopped", deployment.Status);
        }
        
        [Test]
        public async Task Can_Backup()
        {
            var deploymentResponse = await client.PostAsync(new CreateDeploymentRequest
            {
                Version = "3.5.3"
            });

            Assert.IsNotNull(deploymentResponse);

            using (var backupResponse = await client.PostAsync(new BackupRequest
            {
                Id = deploymentResponse.Deployment.Id
            }))
            {
                var bytes = backupResponse.ToBytes();
                Assert.Greater(bytes.Length, 0);
            }
        }
        
        [Test]
        public async Task Can_Restore()
        {
            var deploymentResponse = await client.PostAsync(new CreateDeploymentRequest
            {
                Version = "3.5.3"
            });

            Assert.IsNotNull(deploymentResponse);

            var deployment = deploymentResponse.Deployment;

            var dumpFileInfo = new FileInfo(@"C:\code\barnardos-au\Neo4jManager\src\Neo4jManager.Tests\dbbackup.dump");
            var restoreResponse = client.PostFile<DeploymentResponse>(
                $@"/deployment/{deployment.Id}/Restore", 
                dumpFileInfo, 
                "application/octet-stream");
            
            Assert.IsNotNull(restoreResponse);
                
            var controlResponse = await client.PostAsync(new ControlRequest
            {
                Id = deployment.Id,
                Operation = Operation.Start
            });

            Assert.IsNotNull(controlResponse);
        }
    }
}
