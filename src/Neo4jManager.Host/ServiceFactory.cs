using System;
using Topshelf;

namespace Neo4jManager.Host
{
    public static class ServiceFactory
    {
        public static Topshelf.Host CreateHost(ServiceControl serviceControl, Action<Topshelf.Host> callback = null)
        {
            var host = HostFactory.New(c =>
            {
                c.Service<ServiceControl>(service =>
                {
                    service.ConstructUsing(() => serviceControl);
                    service.WhenStarted((s, h) => s.Start(h));
                    service.WhenStopped((s, h) => s.Stop(h));
                });

                c.RunAsLocalSystem();
                c.SetDescription("Provides a Web API to manage Neo4j instances");
                c.SetDisplayName("Neo4jManager");
                c.SetServiceName("Neo4jManager");

                c.EnablePauseAndContinue();

                c.EnableServiceRecovery(r => { r.RestartService(1); });
            });

            callback?.Invoke(host);

            return host;
        }
    }
}
