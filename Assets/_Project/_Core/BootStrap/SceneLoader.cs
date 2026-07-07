using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class SceneLoader
{
    private readonly Dictionary<string, AsyncOperationHandle<SceneInstance>> _loadedScenesHandlers = new();

    public Dictionary<string, AsyncOperationHandle<SceneInstance>> LoadedScenesHandlers => _loadedScenesHandlers;

    public Dictionary<string, Scene> LoadedScenes = new();

    public IEnumerator LoadScene(AssetReference sceneTarget, System.Action<string> onLoad = null, bool activeOnLoaded = true)
    {
        AsyncOperationHandle<SceneInstance> handle =
            Addressables.LoadSceneAsync(sceneTarget, LoadSceneMode.Additive);

        yield return handle;

        

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            SceneInstance sceneInstance = handle.Result;

            _loadedScenesHandlers.Add(sceneInstance.Scene.name, handle);
            LoadedScenes.Add(sceneInstance.Scene.name, sceneInstance.Scene);

            if (activeOnLoaded)
                SceneManager.SetActiveScene(sceneInstance.Scene);

            onLoad?.Invoke(sceneInstance.Scene.name);
        }
        else
        {
            Debug.LogError($"Failed to load scene {sceneTarget.RuntimeKey}");
        }
    }

    public IEnumerator UnloadScene(string sceneName)
    {
        if (_loadedScenesHandlers.TryGetValue(sceneName, out var handle))
        {
            string unloadedName = handle.Result.Scene.name;

            yield return Addressables.UnloadSceneAsync(handle, true);

            _loadedScenesHandlers.Remove(sceneName);
            LoadedScenes.Remove(sceneName);
            Debug.Log(unloadedName + " unloaded.");
        }
    }

    public void SetActive(string sceneName)
    {
        SceneManager.SetActiveScene(LoadedScenes[sceneName]);
    }
}