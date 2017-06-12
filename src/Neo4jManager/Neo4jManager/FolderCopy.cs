using System.Diagnostics;

namespace Neo4jManager
{
    public class FileCopy : IFileCopy
    {
        public void MirrorFolders(string source, string destination)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "ROBOCOPY.EXE",
                Arguments = $" {source} {destination} /mir /w:15 /r:5 /np"
            };

            using (var process = new Process {StartInfo = processStartInfo})
            {
                process.Start();
                process.WaitForExit();
            }
        }
    }
}
