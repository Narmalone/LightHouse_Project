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
        public string Url { get; set; }
        public string Hash { get; set; }
        public long Size { get; set; }
    }

    public static class ManifestGenerator
    {
        public static void Generate(string basePath, string outputPath)
        {
            var files = Directory.GetFiles(basePath, "*", SearchOption.AllDirectories)
            .Where(path =>
                !path.EndsWith("manifest.json", StringComparison.OrdinalIgnoreCase) &&
                !Path.GetFileName(path).StartsWith(".") && // ignore .git, .gitattributes
                !path.Contains("\\.git") &&
                !path.EndsWith(".meta", StringComparison.OrdinalIgnoreCase)
            )
            .ToArray();

            var manifest = files.Select(file =>
            {
                var relativePath = Path.GetRelativePath(basePath, file).Replace("\\", "/");
                var size = new FileInfo(file).Length;

                // Optionnel : ton tag version GitHub (tu peux le passer en paramètre si tu veux)
                string githubVersion = "V1.0.0";

                // Crée l'URL si le fichier dépasse 100 Mo
                string url = null;
                if (size > 100 * 1024 * 1024)
                {
                    var fileName = Path.GetFileName(relativePath);
                    url = $"https://github.com/Narmalone/Lighthouse-Game-ForLauncher/releases/download/{githubVersion}/{fileName}";
                }

                return new FileMetadata
                {
                    RelativePath = relativePath,
                    Hash = ComputeSHA256(file),
                    Size = size,
                    Url = url
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
