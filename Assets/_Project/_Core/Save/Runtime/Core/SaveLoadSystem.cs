using LightHouse.Core.Save.Sample;
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
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            Bind<Hero, PlayerData>(_gameData.PlayerData);
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
        }

        /// <summary>
        /// Seulement un seul d'un type particulier comme un seul inventaire, un seul joueur etc...
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TData"></typeparam>
        /// <param name="data"></param>
        void Bind<T, TData>(TData data) where T : MonoBehaviour, IBind<TData> where TData : ISaveable, new()
        {
            var entity = FindObjectsByType<T>(FindObjectsSortMode.None).FirstOrDefault();
            if(entity != null)
            {
                if(data == null)
                {
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

        public void NewGame()
        {
            _gameData = new GameData
            { 
                Name = "New Game",
                CurrentLevelName = "ComputerScene"
            };
        }

        public void SaveGame()
        {
            _dataService.Save(_gameData);
            Debug.Log("saving game");
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
    }
}
