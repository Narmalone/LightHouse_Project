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

            foreach (var rule in initialLoadRules)
            {
                yield return LoadAssetsIntoScene(rule.label, rule.sceneName);
                yield return new WaitForSeconds(0.2f);
            }

            yield return LoadScenesByLabel(_sceneMenuLabel);
            SceneManager.SetActiveScene(loadedScenes[_sceneNameMenu]);

            foreach (var rule in menuLoadRules)
            {
                yield return LoadAssetsIntoScene(rule.label, rule.sceneName);
                yield return new WaitForSeconds(0.2f);
            }

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

            SceneManager.SetActiveScene(loadedScenes[_sceneNameBootStrap]);

            if (loadedScenes.TryGetValue(_sceneNameMenu, out var menuScene))
            {
                var unloadHandle = SceneManager.UnloadSceneAsync(menuScene);
                while (!unloadHandle.isDone)
                    yield return null;

                loadedScenes.Remove(_sceneNameMenu);
            }

            yield return LoadScenesByLabel(_sceneGameLabel);

            if (loadedScenes.TryGetValue(_sceneNameGame, out var gameScene))
            {
                SceneManager.SetActiveScene(gameScene);
            }

            foreach (var rule in gameLoadRules)
            {
                yield return LoadAssetsIntoScene(rule.label, rule.sceneName);
            }

            OnGameAssetsLoaded?.Invoke();

            yield return new WaitForSeconds(0.5f);
            loadingScreen.Hide();
        }

        #endregion

        #region From Game To Menu Start Flow
        private IEnumerator HandleReturnToMenu()
        {
            loadingScreen.Show();
            loadingScreen.SetProgress(0f);

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
            yield return LoadScenesByLabel(_sceneMenuLabel);

            if (loadedScenes.TryGetValue(_sceneNameMenu, out var menuScene))
            {
                SceneManager.SetActiveScene(menuScene);
            }

            // Recharger les assets du menu
            foreach (var rule in menuLoadRules)
            {
                yield return LoadAssetsIntoScene(rule.label, rule.sceneName);
                yield return new WaitForSeconds(0.2f);
            }

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

        private IEnumerator LoadAssetsIntoScene(string label, string sceneName)
        {
            if (!loadedScenes.TryGetValue(sceneName, out var scene) || !scene.IsValid())
            {
                Debug.LogError($"[BootStrap] Scène cible '{sceneName}' non chargée ou invalide.");
                yield break;
            }

            var locationsHandle = Addressables.LoadResourceLocationsAsync(label, typeof(GameObject));
            yield return locationsHandle;

            if (!locationsHandle.IsValid() || locationsHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[BootStrap] Échec du chargement des emplacements pour le label '{label}'");
                yield break;
            }

            var locations = locationsHandle.Result;
            int total = locations.Count;
            int loaded = 0;

            foreach (var location in locations)
            {
                loadingScreen.SetProgressName(location.PrimaryKey);
                yield return null;

                var loadHandle = Addressables.LoadAssetAsync<GameObject>(location);
                yield return loadHandle;

                if (loadHandle.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogError($"[BootStrap] Échec du chargement de l'asset '{location.PrimaryKey}' pour le label '{label}'");
                    continue;
                }

                var prefab = loadHandle.Result;
                var instance = Instantiate(prefab);
                SceneManager.MoveGameObjectToScene(instance, scene);
                instance.SetActive(true);

                yield return null;

                loaded++;
                loadingScreen.SetProgress((float)loaded / total);
                yield return null;
            }
        }

        #endregion
    }
}

