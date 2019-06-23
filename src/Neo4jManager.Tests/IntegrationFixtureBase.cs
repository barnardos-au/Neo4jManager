using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver.V1;
using Neo4jManager.ServiceModel;
using NUnit.Framework;
using ServiceStack;

namespace Neo4jManager.Tests
{
    public abstract class IntegrationFixtureBase
    {
        protected const string BaseUri = "http://localhost:2000/";
    
        protected IServiceClient ServiceClient;

        protected ServiceStackHost AppHost;

        [OneTimeSetUp]
        protected virtual void OneTimeSetUp()
        {
            AppHost = new TestAppHost();
            foreach (var appSetting in CustomAppSettings)
            {
                AppHost.AppSettings.Set(appSetting.Key, appSetting.Value);
            }
            
            AppHost.Init();
            AppHost.Start(BaseUri);
            
            ServiceInterface.Helper.ConfigureMappers();
            
            ServiceClient = new JsonServiceClient(BaseUri);
        }

        [OneTimeTearDown]
        protected virtual void OneTimeTearDown()
        {
            AppHost.Dispose();
        }

        protected virtual Dictionary<string, string> CustomAppSettings => new Dictionary<string, string>();
        
        protected async Task DeleteDeployment(string id)
        {
            await ServiceClient.DeleteAsync(new DeploymentRequest
            {
                Id = id, 
                Permanent = true
            });
        }
        
        protected async Task AssertRestoredDatabase(string boltEndpoint)
        {
            using (var driver = GraphDatabase.Driver(boltEndpoint))
            {
                using(var session = driver.Session())
                {
                    var result = await session.RunAsync("MATCH (p:Person) RETURN p.FirstName as FirstName, p.LastName AS LastName");
                    var record = await result.SingleAsync();
                    
                    Assert.AreEqual("Foo", record["FirstName"].As<string>());
                    Assert.AreEqual("Bar", record["LastName"].As<string>());
                }
            }
        }
    }
}
