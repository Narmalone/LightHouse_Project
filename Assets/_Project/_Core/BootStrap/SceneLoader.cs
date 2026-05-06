using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class SceneLoader
{
    private readonly Dictionary<string, Scene> _loadedScenes = new();

    public Dictionary<string, Scene> LoadedScenes => _loadedScenes;

    public IEnumerator LoadByLabel(string label)
    {
        var locationsHandle = Addressables.LoadResourceLocationsAsync(label, typeof(SceneInstance));
        yield return locationsHandle;

        foreach (var loc in locationsHandle.Result)
        {
            var handle = Addressables.LoadSceneAsync(loc, LoadSceneMode.Additive);
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                var scene = handle.Result.Scene;
                _loadedScenes[scene.name] = scene;
            }
        }
    }

    public IEnumerator LoadByLabel(string label, System.Action<string, float> onProgress = null)
    {
        var locationsHandle = Addressables.LoadResourceLocationsAsync(label, typeof(SceneInstance));
        yield return locationsHandle;

        var locations = locationsHandle.Result;
        int total = locations.Count;
        int current = 0;

        foreach (var loc in locations)
        {
            var handle = Addressables.LoadSceneAsync(loc, LoadSceneMode.Additive);

            while (!handle.IsDone)
            {
                onProgress?.Invoke(loc.PrimaryKey, handle.PercentComplete);
                yield return null;
            }

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                var scene = handle.Result.Scene;
                _loadedScenes[scene.name] = scene;
            }

            current++;
            onProgress?.Invoke(loc.PrimaryKey, (float)current / total);
        }
    }

    public IEnumerator Unload(string sceneName)
    {
        if (!_loadedScenes.TryGetValue(sceneName, out var scene))
            yield break;

        var handle = SceneManager.UnloadSceneAsync(scene);
        while (!handle.isDone)
            yield return null;

        _loadedScenes.Remove(sceneName);
    }

    public void SetActive(string sceneName)
    {
        Debug.Log($"Setting active scene: {sceneName}");
        if (_loadedScenes.TryGetValue(sceneName, out var scene))
        {
            SceneManager.SetActiveScene(scene);
        }
    }
}