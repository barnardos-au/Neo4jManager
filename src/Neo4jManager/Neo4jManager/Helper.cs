using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management;
using System.Net;
using System.ServiceProcess;
using System.Text.RegularExpressions;

namespace Neo4jManager
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class Helper
    {
        public static string FindJavaExe()
        {
            const string javaRoot = @"C:\Program Files\Java";
            return Directory.GetFiles(javaRoot, "java.exe", SearchOption.AllDirectories)
                .ToList().OrderByDescending(p => p).FirstOrDefault();
        }

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

        public static void KillNeo4jServices()
        {
            var neo4jServices = ServiceController.GetServices()
                .Where(s => s.ServiceName.Contains("neo4j", StringComparison.OrdinalIgnoreCase) 
                && !s.ServiceName.Contains("neo4jmanager", StringComparison.OrdinalIgnoreCase));

            foreach (var service in neo4jServices)
            {
                SafeAction(() =>
                {
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
                });

                SafeAction(() =>
                {
                    using (var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "SC.EXE",
                            Arguments = $"delete {service.ServiceName}"
                        }
                    })
                    {
                        process.Start();
                        process.WaitForExit(10000);
                    }
                });
            }
        }

        public static void KillJavaProcesses()
        {
            var searcher = new ManagementObjectSearcher(@"SELECT * FROM Win32_Process WHERE CommandLine like '%java%neo4j%'");
            var objects = searcher.Get();
            foreach (var o in objects)
            {
                SafeAction(() =>
                {
                    var id = Convert.ToInt32(o.GetPropertyValue("ProcessId"));
                    using (var p = Process.GetProcessById(id))
                    {
                        p.Kill();
                    }
                });
            }
        }

        public static string GetDescription(this Enum en)
        {
            var type = en.GetType();

            var memInfo = type.GetMember(en.ToString());

            if (memInfo == null || memInfo.Length <= 0) return en.ToString();

            var attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attrs != null && attrs.Length > 0)
            {
                return ((DescriptionAttribute)attrs[0]).Description;
            }

            return en.ToString();
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
