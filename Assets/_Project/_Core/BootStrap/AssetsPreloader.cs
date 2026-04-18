using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AssetPreloader
{
    private readonly Dictionary<string, List<object>> _cache = new();

    public IEnumerator Preload(string label)
    {
        var locationsHandle = Addressables.LoadResourceLocationsAsync(label);
        yield return locationsHandle;

        var list = new List<object>();

        foreach (var loc in locationsHandle.Result)
        {
            var handle = Addressables.LoadAssetAsync<object>(loc);
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                list.Add(handle.Result);
            }
        }

        _cache[label] = list;
    }

    public IEnumerator Preload(string label, System.Action<string, float> onProgress = null)
    {
        var locationsHandle = Addressables.LoadResourceLocationsAsync(label);
        yield return locationsHandle;

        var locations = locationsHandle.Result;

        var list = new List<object>();
        int total = locations.Count;
        int current = 0;

        foreach (var loc in locations)
        {
            var handle = Addressables.LoadAssetAsync<object>(loc);

            while (!handle.IsDone)
            {
                UnityEngine.Debug.Log($"[AssetPreloader] Loading {loc.PrimaryKey}: {handle.PercentComplete * 100}%");
                onProgress?.Invoke(loc.PrimaryKey, handle.PercentComplete);
                yield return null;
            }

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                list.Add(handle.Result);
            }

            current++;
            onProgress?.Invoke(loc.PrimaryKey, (float)current / total);
        }

        _cache[label] = list;
    }

    public T Get<T>(string label, int index = 0) where T : class
    {
        if (!_cache.TryGetValue(label, out var list)) return null;
        return list[index] as T;
    }
}