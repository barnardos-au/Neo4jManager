using System.Diagnostics;

namespace Neo4jManager
{
    public interface IJavaProcessBuilder
    {
        Process GetProcess();
    }
}