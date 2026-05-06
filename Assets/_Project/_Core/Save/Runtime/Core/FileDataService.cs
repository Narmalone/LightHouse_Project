using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LightHouse.Core.Save
{
    public class FileDataService : IDataService
    {
        ISerializer serializer;
        string dataPath;
        string fileExtension;

        public FileDataService(ISerializer serializer)
        {
            this.dataPath = Path.Combine(Application.persistentDataPath, "GameSaves");
            this.fileExtension = "json";
            this.serializer = serializer;

            if(!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }
        }

        private string GetPathToFile(string fileName)
        {
            return Path.Combine(dataPath, string.Concat(fileName, ".", fileExtension));
        }

        public void Save(GameData data, bool overwrite = true)
        {
            string fileLocation = GetPathToFile(data.Name);

            if(!overwrite && File.Exists(fileLocation))
            {
                throw new IOException($"The file '{data.Name}.{fileExtension}' already exists and cannot be overwritten.");
            }

            File.WriteAllText(fileLocation, serializer.Serialize(data));
        }

        public GameData Load(string name)
        {
            string fileLocation = GetPathToFile(name);

            if (!File.Exists(fileLocation))
            {
                throw new ArgumentException($"No persisted Game Data with name '{name}'");
            }

            return serializer.Deserialize<GameData>(File.ReadAllText(fileLocation));
        }

        public void Delete(string name)
        {
            string fileLocation = GetPathToFile(name);
            if (File.Exists(fileLocation))
            {
                File.Delete(fileLocation);
            }
        }

        public void DeleteAll()
        {
            foreach(string filepath in Directory.GetFiles(dataPath))
            {
                File.Delete(filepath);
            }
        }

        public IEnumerable<string> ListSaves()
        {
            foreach (string filepath in Directory.EnumerateFiles(dataPath))
            {
                if(Path.GetExtension(filepath) == $".{fileExtension}")
                {
                    yield return Path.GetFileNameWithoutExtension(filepath);
                }
            }
        }

        // FileDataService.cs
        public IEnumerable<GameData> ListSavesMetadata()
        {
            foreach (var name in ListSaves())
            {
                GameData data = null;
                try { data = Load(name); }
                catch { /* fichier corrompu, on skip */ }

                if (data != null) yield return data;
            }
        }
    }

}
