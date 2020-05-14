using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Neo4jManager.ServiceModel;
using NUnit.Framework;
using ServiceStack;

namespace Neo4jManager.Tests
{
    [TestFixture]
    public class IntegrationTest : IntegrationFixtureBase
    {
        [Test]
        public async Task Can_Install_And_Start()
        {
            var deploymentResponse = await ServiceClient.PostAsync(new CreateDeploymentRequest
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

            var controlResponse = await ServiceClient.PostAsync(new ControlRequest
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
            
            controlResponse = await ServiceClient.PostAsync(new ControlRequest
            {
                Id = deployment.Id,
                Operation = Operation.Stop
            });

            Assert.IsNotNull(controlResponse);

            deployment = controlResponse.Deployment;
            Assert.IsNotNull(deployment);
            Assert.AreEqual("Stopped", deployment.Status);

            await DeleteDeployment(deployment.Id);
        }
        
        [Test]
        public async Task Can_Backup()
        {
            var deploymentResponse = await ServiceClient.PostAsync(new CreateDeploymentRequest
            {
                Version = "3.5.3",
                AutoStart = true
            });

            Assert.IsNotNull(deploymentResponse);

            var deployment = deploymentResponse.Deployment;

            using (var backupResponse = await ServiceClient.PostAsync(new BackupRequest
            {
                Id = deployment.Id
            }))
            {
                var bytes = backupResponse.ToBytes();
                Assert.Greater(bytes.Length, 0);
            }
            
            await DeleteDeployment(deployment.Id);
        }
        
        [Test]
        public async Task Can_Restore()
        {
            var deploymentResponse = await ServiceClient.PostAsync(new CreateDeploymentRequest
            {
                Version = "3.5.3",
                AutoStart = true
            });

            Assert.IsNotNull(deploymentResponse);

            var deployment = deploymentResponse.Deployment;

            var dumpFile = Path.Combine(AppContext.BaseDirectory, "dbbackup.dump"); 
            var dumpFileInfo = new FileInfo(dumpFile);
            var restoreResponse = ServiceClient.PostFile<DeploymentResponse>(
                $@"/deployment/{deployment.Id}/Restore", 
                dumpFileInfo, 
                "application/octet-stream");
            
            Assert.IsNotNull(restoreResponse);
                
            var controlResponse = await ServiceClient.PostAsync(new ControlRequest
            {
                Id = deployment.Id,
                Operation = Operation.Start
            });

            Assert.IsNotNull(controlResponse);

            await AssertRestoredDatabase(deployment.Endpoints.BoltEndpoint);
            
            await DeleteDeployment(deployment.Id);
        }

        [Test]
        public async Task Can_Install_With_Specified_Dump_File_From_Url()
        {
            var deploymentResponse = await ServiceClient.PostAsync(new CreateDeploymentRequest
            {
                Version = "3.5.3",
                RestoreDumpFileUrl = Path.Combine(AppContext.BaseDirectory, "dbbackup.dump"),
                AutoStart = true
            });

            Assert.IsNotNull(deploymentResponse);

            var deployment = deploymentResponse.Deployment;

            await AssertRestoredDatabase(deployment.Endpoints.BoltEndpoint);
            
            await DeleteDeployment(deployment.Id);
        }
        
        [Test]
        public async Task Can_Create_Concurrent_Deployments()
        {
            var request = new CreateDeploymentRequest
            {
                Version = "3.5.3",
                AutoStart = true
            };

            var createTasks = new List<Task<DeploymentResponse>>();
            for (var i = 0; i < 4; i++)
            {
                createTasks.Add(ServiceClient.PostAsync(request));
            }

            await Task.WhenAll(createTasks);

            var deployments = createTasks.Select(t => t.Result.Deployment).ToList();
            foreach (var deployment in deployments)
            {
                Assert.AreEqual("Started", deployment.Status);
            }

            var distinctEndpoints = deployments.Select(d => d.Endpoints.HttpEndpoint).Distinct();
            Assert.AreEqual(4, distinctEndpoints.Count());

            foreach (var deployment in deployments)
            {
                await DeleteDeployment(deployment.Id);
            }
        }
    }
}
