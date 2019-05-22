using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ServiceStack;

namespace Neo4jManager
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class Helper
    {
        public static void Download(Neo4jVersion neo4jVersion, string neo4jBasePath)
        {
            var zipFile = Path.Combine(neo4jBasePath, neo4jVersion.ZipFileName);

            if (File.Exists(zipFile)) return;

            Console.WriteLine($"Downloading Neo4j from {neo4jVersion.DownloadUrl}");

            var fileInfo = new FileInfo(zipFile);
            Directory.CreateDirectory(fileInfo.DirectoryName);

            using (var webClient = new WebClient())
            {
                webClient.DownloadFile(neo4jVersion.DownloadUrl, zipFile);
            }
        }

        public static async Task DownloadAsync(Neo4jVersion neo4jVersion, string neo4jBasePath)
        {
            var zipFile = Path.Combine(neo4jBasePath, neo4jVersion.ZipFileName);

            if (File.Exists(zipFile)) return;

            Console.WriteLine($"Downloading Neo4j from {neo4jVersion.DownloadUrl}");

            var fileInfo = new FileInfo(zipFile);
            Directory.CreateDirectory(fileInfo.DirectoryName);

            var fileBytes = await neo4jVersion.DownloadUrl.GetBytesFromUrlAsync();
            using (var SourceStream = File.Open(zipFile, FileMode.OpenOrCreate))
            {
                SourceStream.Seek(0, SeekOrigin.End);
                await SourceStream.WriteAsync(fileBytes, 0, fileBytes.Length);
            }
        }

        public static void Extract(Neo4jVersion neo4jVersion, string neo4jBasePath)
        {
            var zipFile = Path.Combine(neo4jBasePath, neo4jVersion.ZipFileName);
            var extractFolder = Path.Combine(neo4jBasePath, Path.GetFileNameWithoutExtension(neo4jVersion.ZipFileName));

            if (Directory.Exists(extractFolder)) return;

            Console.WriteLine($"Extracting Neo4j from {neo4jVersion.ZipFileName} to {extractFolder}");

            ZipFile.ExtractToDirectory(zipFile, extractFolder);
        }

        public static void SafeDelete(string path)
        {
            SafeAction(() => Directory.Delete(path, true));
        }

        public static void DownloadFile(string downloadFileUrl, string destinationFolder)
        {
            Console.WriteLine($"Downloading file from {downloadFileUrl}");
            using (var webClient = new WebClient())
            {
                Uri uri = new Uri(downloadFileUrl);
                var fileName = System.IO.Path.GetFileName(uri.LocalPath);
                webClient.DownloadFile(downloadFileUrl, $@"{destinationFolder}\{fileName}");
            }
        }

        public static void CopyDeployment(Neo4jVersion neo4jVersion, string neo4jBasePath, string targetDeploymentPath)
        {
            var extractFolder = Path.Combine(neo4jBasePath, Path.GetFileNameWithoutExtension(neo4jVersion.ZipFileName));

            new FileCopy().MirrorFolders(extractFolder, targetDeploymentPath);
        }

        public static string GenerateValidFolderName(string folderName)
        {
            var invalidFileNameChars = new string(Path.GetInvalidFileNameChars());
            var invalidChars = Regex.Escape(invalidFileNameChars);
            var invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return Regex.Replace(folderName, invalidRegStr, "_");
        }

        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source != null && toCheck != null && source.IndexOf(toCheck, comp) >= 0;
        }

        public static void SafeAction(Action action)
        {
            try
            {
                action();
            }
            catch
            {
            }
        }
    }
}
