using System.IO;
using System.Linq;

namespace Neo4jManager
{
    public class ZuluJavaResolver : IJavaResolver
    {
        public string GetJavaPath()
        {
            const string javaRoot = @"C:\Program Files\Zulu";
            return Directory.GetFiles(javaRoot, "java.exe", SearchOption.AllDirectories)
                .ToList().OrderByDescending(p => p).FirstOrDefault();
        }
    }
}
