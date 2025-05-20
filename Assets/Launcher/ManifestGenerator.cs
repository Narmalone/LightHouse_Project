using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
namespace LightHouse.Launcher
{
    public class FileMetadata
    {
        public string RelativePath { get; set; }
        public string Hash { get; set; }
        public long Size { get; set; }
    }

    public static class ManifestGenerator
    {
        public static void Generate(string basePath, string outputPath)
        {
            var files = Directory.GetFiles(basePath, "*", SearchOption.AllDirectories);

            var manifest = files.Select(file =>
            {
                var relativePath = Path.GetRelativePath(basePath, file).Replace("\\", "/");
                return new FileMetadata
                {
                    RelativePath = relativePath,
                    Hash = ComputeSHA256(file),
                    Size = new FileInfo(file).Length
                };
            }).ToList();

            var json = JsonConvert.SerializeObject(manifest, Formatting.Indented);
            File.WriteAllText(outputPath, json);
            Console.WriteLine("Manifest généré à : " + outputPath);
        }

        private static string ComputeSHA256(string filePath)
        {
            using var sha = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            var hash = sha.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
