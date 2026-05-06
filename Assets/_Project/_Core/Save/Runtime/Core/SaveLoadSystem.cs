using LightHouse.Core.Player;
using LightHouse.Core.Save.Sample;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LightHouse.Core.Save
{
    public class SaveLoadSystem : PersistentSingleton<SaveLoadSystem>
    {
        [SerializeField] private GameData _gameData;
        public GameData GameData => _gameData;

        private IDataService _dataService;

        protected override void Awake()
        {
            base.Awake();
            _dataService = new FileDataService(new JsonSerializer());
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if(arg0.name == "ComputerScene")
            {
                Bind<PlayerController, PlayerData>(ref _gameData.PlayerData);
            }
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        /// <summary>
        /// Seulement un seul d'un type particulier comme un seul inventaire, un seul joueur etc...
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TData"></typeparam>
        /// <param name="data"></param>
        void Bind<T, TData>(ref TData data) where T : MonoBehaviour, IBind<TData> where TData : ISaveable, new()
        {
            var entity = FindObjectsByType<T>(FindObjectsSortMode.None).FirstOrDefault();
            if(entity != null)
            {
                if(data == null)
                {
                    Debug.Log("data is null, creating new one");
                    data = new TData { Id = entity.Id }; 
                }
                entity.Bind(data);
            }
        }

        /// <summary>
        /// Pour plusieures entitées comme plusieurs ennemis, plusieurs bateaux etc...
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TData"></typeparam>
        /// <param name="datas"></param>
        void Bind<T, TData>(List<TData> datas) where T : MonoBehaviour, IBind<TData> where TData : ISaveable, new()
        {
            var entities = FindObjectsByType<T>(FindObjectsSortMode.None);
            foreach(var entity in entities)
            {
                var data = datas.FirstOrDefault(d => d.Id == entity.Id);
                if(data == null)
                {
                    data = new TData { Id = entity.Id };
                    datas.Add(data);
                }
                entity.Bind(data);
            }
        }

        void BindList<T, TData>(List<TData> datas)
            where T : MonoBehaviour, IBind<TData>
            where TData : ISaveable, new()
                {
                    var entities = FindObjectsByType<T>(FindObjectsSortMode.None);
                    foreach (var entity in entities)
                    {
                        var data = datas.FirstOrDefault(d => d.Id == entity.Id);
                        if (data == null)
                        {
                            data = new TData { Id = entity.Id };
                            datas.Add(data);
                        }
                        entity.Bind(data);
                    }

                    // ✅ AJOUT : désactiver les objets qui étaient ramassés/détruits
                    foreach (var data in datas.Where(d => entities.All(e => e.Id != d.Id)))
                    {
                        // Logique à définir selon ton besoin (ex: ne pas spawner)
                    }
                }

        public void NewGame()
        {
            _gameData = new GameData
            { 
                Name = $"SaveNameRandom_{UnityEngine.Random.Range(0, 999)}",
                //CurrentLevelName = "ComputerScene"
            };
        }

        public void SaveGame()
        {
            foreach (var capturable in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                                .OfType<ICapturableState>())
            {
                capturable.CaptureState();
            }

            _gameData.LastSaveTime = DateTime.UtcNow.ToString("o");
            _dataService.Save(_gameData);
        }

        public void LoadGame(string gameName)
        {
            _gameData = _dataService.Load(gameName);

            if(string.IsNullOrEmpty(_gameData.CurrentLevelName))
            {
                _gameData.CurrentLevelName = "GameScene";
            }
            //we can call the load scene if we want here
            //SceneManager.LoadScene(_gameData.CurrentLevelName);
        }

        public void ReloadGame() => LoadGame(_gameData.Name);

        public void DeleteGame(string gameName)
        {
            _dataService.Delete(gameName);
        }

        public void DeleteAllGames()
        {
            _dataService.DeleteAll();
        }

        public List<string> GetSaveList()
        {
            return _dataService.ListSaves().ToList();
        }

        // SaveLoadSystem.cs
        public List<GameData> GetSaveMetadataList()
        {
            return _dataService.ListSavesMetadata().ToList();
        }
    }
}
