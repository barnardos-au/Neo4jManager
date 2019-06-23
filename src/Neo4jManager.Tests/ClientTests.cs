using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Neo4j.Driver.V1;
using Neo4jManager.Client;
using NUnit.Framework;
using ServiceStack;

namespace Neo4jManager.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.None)]
    public class ClientTests : IntegrationFixtureBase
    {
        private INeo4jManagerClient client;

        [OneTimeSetUp]
        protected override void OneTimeSetUp()
        {
            base.OneTimeSetUp();
            
            client = new Neo4jManagerClient(BaseUri);
        }

        [SetUp]
        public void SetUp()
        {
            client.DeleteAll(true);
        }
        
        [Test]
        public async Task Can_Get_Versions()
        {
            // Act
            var versions = await client.GetVersionsAsync();
            
            // Assert
            Assert.Greater(versions.Count(), 0);
        }

        [Test]
        public async Task Can_Get_Deployments()
        {
            // Arrange
            var deployment1 = await client.CreateAsync("3.5.3");
            var deployment2 = await client.CreateAsync("3.5.3");
            
            // Act
            var deployments = await client.GetDeploymentsAsync();

            // Assert
            Assert.AreEqual(2, deployments.Count());

            await client.DeleteAsync(deployment1.Id);
            await client.DeleteAsync(deployment2.Id);
        }

        [Test]
        public async Task Can_Get_Deployment()
        {
            // Arrange
            var deployment = await client.CreateAsync("3.5.3");
            
            // Act
            var fetchedDeployment = await client.GetDeploymentAsync(deployment.Id);

            // Assert
            Assert.AreEqual(deployment.Id, fetchedDeployment.Id);
            Assert.AreEqual(deployment.Status, fetchedDeployment.Status);
            Assert.AreEqual(deployment.DataPath, fetchedDeployment.DataPath);
            Assert.AreEqual(deployment.ExpiresOn, fetchedDeployment.ExpiresOn);

            await client.DeleteAsync(deployment.Id);
        }

        [Test]
        public async Task Can_Delete_All()
        {
            // Arrange
            await client.CreateAsync("3.5.3");
            await client.CreateAsync("3.5.3");
            
            var deployments = await client.GetDeploymentsAsync();
            
            Assert.Greater(deployments.Count(), 0);

            // Act
            await client.DeleteAllAsync(true);
            
            // Assert
            deployments = await client.GetDeploymentsAsync();

            Assert.AreEqual(0, deployments.Count());
        }

        [Test]
        public async Task Can_Delete()
        {
            // Arrange
            var deployment = await client.CreateAsync("3.5.3");
            
            Assert.IsNotNull(deployment);
            Assert.AreEqual("Stopped", deployment.Status);
            
            await client.DeleteAsync(deployment.Id);

            var deletedDeployment = await client.GetDeploymentAsync(deployment.Id);
            
            Assert.AreEqual("Deleted", deletedDeployment.Status);
        }

        [Test]
        public async Task Can_Create()
        {
            // Act
            var deployment = await client.CreateAsync("3.5.3");

            // Assert
            Assert.IsNotNull(deployment);
            Assert.AreEqual("Stopped", deployment.Status);
            
            await client.DeleteAsync(deployment.Id);
        }

        [Test]
        public async Task Can_Start()
        {
            // Arrange
            var deployment = await client.CreateAsync("3.5.3");

            // Act
            await client.StartAsync(deployment.Id);

            // Assert
            var startedDeployment = await client.GetDeploymentAsync(deployment.Id);
            
            Assert.AreEqual("Started", startedDeployment.Status);
            
            await client.DeleteAsync(deployment.Id);
        }

        [Test]
        public async Task Can_Stop()
        {
            // Arrange
            var deployment = await client.CreateAsync("3.5.3", autoStart: true);

            Assert.AreEqual("Started", deployment.Status);

            // Act
            await client.StopAsync(deployment.Id);

            // Assert
            var stoppedDeployment = await client.GetDeploymentAsync(deployment.Id);
            
            Assert.AreEqual("Stopped", stoppedDeployment.Status);
            
            await client.DeleteAsync(deployment.Id);
        }

        [Test]
        public async Task Can_Configure_And_Restart()
        {
            // Arrange
            var deployment = await client.CreateAsync("3.5.3", autoStart: true);

            Assert.AreEqual("Started", deployment.Status);

            await client.ConfigureAsync(deployment.Id, "neo4j.conf", "dbms.read_only", "true");
            
            // Act
            await client.RestartAsync(deployment.Id);

            // Assert
            var restartedDeployment = await client.GetDeploymentAsync(deployment.Id);
            
            Assert.AreEqual("Started", restartedDeployment.Status);
            
            var exception = Assert.ThrowsAsync<ClientException>(async () =>
            {
                using (var driver = GraphDatabase.Driver(restartedDeployment.Endpoints.BoltEndpoint))
                {
                    using(var session = driver.Session())
                    {
                        await session.RunAsync("CREATE (t:Test)");
                    }
                }
            });
            
            Assert.AreEqual("No write operations are allowed on this database. This is a read only Neo4j instance.", exception.Message);
            
            await client.DeleteAsync(deployment.Id);
        }

        [Test]
        public async Task Can_Clear()
        {
            // Arrange
            var deployment = await client.CreateAsync("3.5.3", autoStart: true);

            Assert.AreEqual("Started", deployment.Status);

            using (var driver = GraphDatabase.Driver(deployment.Endpoints.BoltEndpoint))
            {
                using (var session = driver.Session())
                {
                    await session.RunAsync("CREATE (t:Test { Name: 'Foo'})");
                    var result = await session.RunAsync("MATCH (t:Test) RETURN COUNT(t) AS NodeCount");
                    var record = await result.SingleAsync();

                    Assert.AreEqual(1, record["NodeCount"].As<int>());
                }
            }

            // Act
            await client.ClearAsync(deployment.Id);

            // Assert
            using (var driver = GraphDatabase.Driver(deployment.Endpoints.BoltEndpoint))
            {
                using (var session = driver.Session())
                {
                    var result = await session.RunAsync("MATCH (t:Test) RETURN COUNT(t) AS NodeCount");
                    var record = await result.SingleAsync();
                    
                    Assert.AreEqual(0, record["NodeCount"].As<int>());
                }
            }

            await client.DeleteAsync(deployment.Id);
        }

        [Test]
        public async Task Can_Backup()
        {
            // Arrange
            var deployment = await client.CreateAsync("3.5.3", autoStart: true);

            Assert.AreEqual("Started", deployment.Status);

            // Act / Assert
            using (var stream = await client.BackupAsync(deployment.Id))
            {
                var bytes = stream.ToBytes();
                Assert.Greater(bytes.Length, 0);
            }

            deployment = await client.GetDeploymentAsync(deployment.Id);
            
            Assert.AreEqual("Started", deployment.Status);
            
            await client.DeleteAsync(deployment.Id);
        }

        [Test]
        public async Task Can_Restore()
        {
            // Arrange
            var deployment = await client.CreateAsync("3.5.3", autoStart: true);

            Assert.AreEqual("Started", deployment.Status);

            var dumpFile = Path.Combine(AppContext.BaseDirectory, "dbbackup.dump"); 
            var dumpFileInfo = new FileInfo(dumpFile);
            
            // Act
            using (var fileStream = dumpFileInfo.OpenRead())
            {
                var restoreResponse = await client.RestoreAsync(deployment.Id, fileStream);
                Assert.IsNotNull(restoreResponse);
            }

            // Assert
            await AssertRestoredDatabase(deployment.Endpoints.BoltEndpoint);
            
            deployment = await client.GetDeploymentAsync(deployment.Id);
            
            Assert.AreEqual("Started", deployment.Status);

            await client.DeleteAsync(deployment.Id);
        }
    }
}
