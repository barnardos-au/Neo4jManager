using System.IO;
using System.Linq;

namespace Neo4jManager
{
    public class OracleJavaResolver : IJavaResolver
    {
        public string GetJavaPath()
        {
            const string javaRoot = @"C:\Program Files\Java";
            return Directory.GetFiles(javaRoot, "java.exe", SearchOption.AllDirectories)
                .ToList().OrderByDescending(p => p).FirstOrDefault();
        }
    }
}
