using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using LightHouse.CustomAttributes;
using LightHouse.Game.Loading;

namespace LightHouse.Game.BootStrap
{
    [Serializable]
    public class AddressableLoadRule
    {
        [AddressableLabelSelector] public string label;
        [SceneSelector] public string sceneName;
    }

    public class BootStrap : MonoBehaviour
    {
        #region ▌Static & Events
        public static BootStrap Instance;
        public static event Action OnGameAssetsLoaded;
        #endregion

        #region ▌Serialized Fields

        [Header("Scenes")]
        [AddressableLabelSelector] public string _sceneMenuLabel;
        [AddressableLabelSelector] public string _sceneGameLabel;

        [SceneSelector] public string _sceneNameBootStrap;
        [SceneSelector] public string _sceneNameMenu;
        [SceneSelector] public string _sceneNameGame;

        [Header("UI")]
        [SerializeField] private LoadingScreen loadingScreen;

        [Header("Load Rules")]
        public List<AddressableLoadRule> initialLoadRules;
        public List<AddressableLoadRule> menuLoadRules;
        public List<AddressableLoadRule> gameLoadRules;

        #endregion

        #region ▌Private Fields

        private readonly Dictionary<string, Scene> loadedScenes = new();

        private readonly Dictionary<string, List<GameObject>> preloadedAssets = new();

        #endregion

        #region ▌Unity Lifecycle

        private void Awake()
        {
            Instance = this;
        }

        private IEnumerator Start()
        {
            loadedScenes.Add(this.gameObject.scene.name, this.gameObject.scene);
            loadingScreen.Show();
            loadingScreen.SetProgress(0f);

            foreach (AddressableLoadRule rule in initialLoadRules)
            {
                yield return PreloadAssets(rule.label, rule.sceneName);
            }

            InstantiateAssetsForScene(_sceneNameBootStrap);

            yield return LoadScenesByLabel(_sceneMenuLabel);
            SceneManager.SetActiveScene(loadedScenes[_sceneNameMenu]);

            foreach (AddressableLoadRule rule in menuLoadRules)
            {
                yield return PreloadAssets(rule.label, rule.sceneName);
            }

            foreach (AddressableLoadRule rule in gameLoadRules)
            {
                yield return PreloadAssets(rule.label, rule.sceneName);
            }
            InstantiateAssetsForScene(_sceneNameMenu);

            loadingScreen.Hide();
        }

        #endregion

        #region ▌Public API

        public void StartGame()
        {
            StartCoroutine(HandleGameStart());
        }
        public void ReturnToMenu()
        {
            StartCoroutine(HandleReturnToMenu());
        }
        #endregion

        #region ▌Game Start Flow

        private IEnumerator HandleGameStart()
        {
            loadingScreen.Show();
            loadingScreen.SetProgress(0f);
            loadingScreen.SetProgressName("Retour au Bootstrap");

            // Revenir à la scène Bootstrap
            SceneManager.SetActiveScene(loadedScenes[_sceneNameBootStrap]);

            // Décharger le menu
            if (loadedScenes.TryGetValue(_sceneNameMenu, out var menuScene))
            {
                var unloadHandle = SceneManager.UnloadSceneAsync(menuScene);
                while (!unloadHandle.isDone)
                    yield return null;

                loadedScenes.Remove(_sceneNameMenu);
            }

            // Charger la scène de jeu
            loadingScreen.SetProgressName("Chargement de la scène de jeu");
            yield return LoadScenesByLabel(_sceneGameLabel);

            if (loadedScenes.TryGetValue(_sceneNameGame, out var gameScene))
            {
                SceneManager.SetActiveScene(gameScene);
            }

            // Instancier les assets préchargés de la scène de jeu
            loadingScreen.SetProgressName("Initialisation des assets de jeu");
            InstantiateAssetsForScene(_sceneNameGame);

            OnGameAssetsLoaded?.Invoke();
            loadingScreen.Hide();
        }


        #endregion

        #region From Game To Menu Start Flow
        private IEnumerator HandleReturnToMenu()
        {
            loadingScreen.Show();
            loadingScreen.SetProgress(0f);
            loadingScreen.SetProgressName("Retour au Bootstrap");

            SceneManager.SetActiveScene(loadedScenes[_sceneNameBootStrap]);

            // Décharger la scène de jeu
            if (loadedScenes.TryGetValue(_sceneNameGame, out var gameScene))
            {
                var unloadHandle = SceneManager.UnloadSceneAsync(gameScene);
                while (!unloadHandle.isDone)
                    yield return null;

                loadedScenes.Remove(_sceneNameGame);
            }

            // Charger la scène du menu
            loadingScreen.SetProgressName("Chargement du menu");
            yield return LoadScenesByLabel(_sceneMenuLabel);

            if (loadedScenes.TryGetValue(_sceneNameMenu, out var menuScene))
            {
                SceneManager.SetActiveScene(menuScene);
            }

            // Instancier les assets préchargés du menu
            loadingScreen.SetProgressName("Initialisation du menu");
            InstantiateAssetsForScene(_sceneNameMenu);

            yield return new WaitForSeconds(0.5f);
            loadingScreen.Hide();
        }

        #endregion

        #region ▌Scene Loading
        private IEnumerator LoadScenesByLabel(string label)
        {
            var handle = Addressables.LoadSceneAsync(label, LoadSceneMode.Additive);
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                var scene = handle.Result.Scene;
                loadedScenes[scene.name] = scene;
                Debug.Log($"[BootStrap] Scène cible '{scene.name}' trouvée avec succès initialization.");
            }
        }

        #endregion

        #region ▌Asset Loading

        private IEnumerator PreloadAssets(string label, string sceneName)
        {
            var locationsHandle = Addressables.LoadResourceLocationsAsync(label, typeof(GameObject));
            yield return locationsHandle;

            if (!locationsHandle.IsValid() || locationsHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[BootStrap] Échec du chargement des emplacements pour le label '{label}'");
                yield break;
            }

            var locations = locationsHandle.Result;
            var list = new List<GameObject>();
            int total = locations.Count;
            int loaded = 0;

            foreach (var location in locations)
            {
                loadingScreen.SetProgressName(location.PrimaryKey);
                yield return null;

                var loadHandle = Addressables.LoadAssetAsync<GameObject>(location);
                yield return loadHandle;

                if (loadHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    list.Add(loadHandle.Result);
                    loaded++;
                }
                else
                {
                    Debug.LogWarning($"[BootStrap] Échec du preload de l'asset '{location.PrimaryKey}'");
                }

                loadingScreen.SetProgress((float)loaded / total);
                yield return null;
            }

            if (!preloadedAssets.ContainsKey(sceneName))
                preloadedAssets[sceneName] = new List<GameObject>();

            preloadedAssets[sceneName].AddRange(list);
        }

        private void InstantiateAssetsForScene(string sceneName)
        {
            if (!preloadedAssets.TryGetValue(sceneName, out var prefabs)) return;
            if (!loadedScenes.TryGetValue(sceneName, out var scene)) return;

            foreach (var prefab in prefabs)
            {
                var instance = Instantiate(prefab);
                SceneManager.MoveGameObjectToScene(instance, scene);
                instance.SetActive(true);
            }
        }
        #endregion
    }
}

