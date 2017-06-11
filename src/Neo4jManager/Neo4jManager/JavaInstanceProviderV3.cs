using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Neo4jManager
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class JavaInstanceProviderV3 : INeo4jInstanceProvider
    {
        private readonly IJavaProcessBuilder javaProcessBuilder;
        private readonly Neo4jOptions options;

        private Process process;

        public JavaInstanceProviderV3(IJavaProcessBuilder javaProcessBuilder, Neo4jOptions options)
        {
            this.javaProcessBuilder = javaProcessBuilder;
            this.options = options;
        }

        public void Start()
        {
            if (process == null)
            {
                process = javaProcessBuilder.GetProcess();
                process.Start();
                return;
            }

            if (process.HasExited)
                process.Start();
        }

        public void Stop()
        {
            if (process == null || process.HasExited) return;

            process.Kill();
        }

        public Neo4jEndpoints Endpoints => options.Endpoints;

        public void Dispose()
        {
            if (process != null && !process.HasExited)
            {
                process.Kill();
            }

            process?.Dispose();
        }
    }
}
