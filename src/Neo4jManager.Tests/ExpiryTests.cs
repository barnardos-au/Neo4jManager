using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Neo4jManager.ServiceModel;
using NUnit.Framework;
using ServiceStack;

namespace Neo4jManager.Tests
{
    [TestFixture]
    public class ExpiryTests : IntegrationFixtureBase
    {
        protected override Dictionary<string, string> CustomAppSettings => new Dictionary<string, string>
        {
            { AppSettingsKeys.WatchDogInterval, "00:00:05" },
            { AppSettingsKeys.ExpiryPeriod, "00:00:30" },
        };

        [Test]
        public async Task Expired_Instance_Should_Be_Disposed()
        {
            var deploymentResponse = await ServiceClient.PostAsync(new CreateDeploymentRequest
            {
                Version = "3.5.3",
                AutoStart = true,
                LeasePeriod = TimeSpan.FromSeconds(30)
            });

            Assert.IsNotNull(deploymentResponse);
            var deployment = deploymentResponse.Deployment;
            Assert.IsNotNull(deployment);
            Assert.AreEqual("Started", deployment.Status);

            var deploymentPath = Path.Combine(
                AppHost.AppSettings.GetString(AppSettingsKeys.Neo4jBasePath),
                "deployments",
                deployment.Id);
            
            Assert.IsTrue(Directory.Exists(deploymentPath), "Deployment path should exist");

            // Act / Assert: that deployment instance has expired
            var count = 0;
            while (true)
            {
                var fetchedDeployment = ServiceClient.Get(new DeploymentRequest {Id = deployment.Id});
                if (fetchedDeployment.Deployment.Status == "Deleted") break;
                
                count++;
                if (count > 20)
                    Assert.Fail("Did not expire deployment instance");

                await Task.Delay(TimeSpan.FromSeconds(5));
            }

            // Act / Assert: can take backup
            using (var backupResponse = await ServiceClient.PostAsync(new BackupRequest
            {
                Id = deployment.Id,
                ObtainLastBackup = true
            }))
            {
                var bytes = backupResponse.ToBytes();
                Assert.Greater(bytes.Length, 0);
            }

            // Act / Assert: that deployment instance has been deleted and folder cleared
            count = 0;
            while (true)
            {
                try
                {
                    ServiceClient.Get(new DeploymentRequest {Id = deployment.Id});
                }
                catch (WebServiceException webServiceException)
                {
                    Assert.IsTrue(webServiceException.Message.EndsWith("not found"));
                    break;
                }
                
                count++;
                if (count > 20)
                    Assert.Fail("Did not expire deployment instance");

                await Task.Delay(TimeSpan.FromSeconds(5));
            }
            
            Assert.IsFalse(Directory.Exists(deploymentPath), $"Deployment path: {deploymentPath} should be deleted");
        }
    }
}
