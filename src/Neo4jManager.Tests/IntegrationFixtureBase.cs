using NUnit.Framework;
using ServiceStack;

namespace Neo4jManager.Tests
{
    public abstract class IntegrationFixtureBase
    {
        protected const string BaseUri = "http://localhost:2000/";
    
        private ServiceStackHost appHost;

        [OneTimeSetUp]
        protected virtual void OneTimeSetUp()
        {
            appHost = new TestAppHost()
                .Init()
                .Start(BaseUri);
            
            ServiceInterface.Helper.ConfigureMappers();
        }

        [OneTimeTearDown]
        protected virtual void OneTimeTearDown()
        {
            appHost.Dispose();
        }
    }
}
