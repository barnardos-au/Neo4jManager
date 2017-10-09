using System;
using System.Diagnostics;
using System.Management;

namespace Neo4jManager.Host
{
    public static class HostHelper
    {
        public static void KillJavaProcesses()
        {
            var searcher = new ManagementObjectSearcher(@"SELECT * FROM Win32_Process WHERE CommandLine like '%java%neo4j%'");
            var objects = searcher.Get();
            foreach (var o in objects)
            {
                Helper.SafeAction(() =>
                {
                    var id = Convert.ToInt32(o.GetPropertyValue("ProcessId"));
                    using (var p = Process.GetProcessById(id))
                    {
                        p.Kill();
                    }
                });
            }
        }
    }
}
