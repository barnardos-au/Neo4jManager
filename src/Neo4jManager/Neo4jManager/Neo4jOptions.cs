using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Neo4jManager
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Neo4jOptions
    {
        public Neo4jOptions()
        {
            Parameters = new Dictionary<string, string>();
        }

        public string HeapInitialSize { get; set; }
        public string HeapMaxSize { get; set; }
        public string PageCacheSize { get; set; }
        public Dictionary<string, string> Parameters { get; }
    }
}
