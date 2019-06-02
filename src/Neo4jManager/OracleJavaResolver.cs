using System.IO;
using System.Linq;

namespace Neo4jManager
{
    public class OracleJavaResolver : IJavaResolver
    {
        private const string JavaRoot = @"C:\Program Files\Java";

        public string GetJavaPath()
        {
            return GetPath("java.exe");
        }

        public string GetToolsPath()
        {
            return GetPath("tools.jar");
        }

        private static string GetPath(string searchPattern)
        {
            return Directory.GetFiles(JavaRoot, searchPattern, SearchOption.AllDirectories)
                .ToList().OrderByDescending(p => p).FirstOrDefault();
        }
    }
}