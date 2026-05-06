using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AssetPreloader
{
    private readonly Dictionary<string, List<object>> _cache = new();
    private readonly List<GameObject> _instantiated = new();

    // ─── Surcharge simple sans progression ───────────────────────────────────
    public IEnumerator Preload(string label, bool instantiateOnLoad = false, bool dontDestroyOnLoad = false)
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

                if (instantiateOnLoad)
                    TryInstantiate(handle.Result, dontDestroyOnLoad);
            }
        }

        _cache[label] = list;
    }

    // ─── Surcharge avec progression ──────────────────────────────────────────
    public IEnumerator Preload(
        string label,
        System.Action<string, float> onProgress,
        bool instantiateOnLoad = false,
        bool dontDestroyOnLoad = false)
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
                onProgress?.Invoke(loc.PrimaryKey, handle.PercentComplete);
                yield return null;
            }

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                list.Add(handle.Result);
                Debug.Log(instantiateOnLoad);
                if (instantiateOnLoad)
                    TryInstantiate(handle.Result, dontDestroyOnLoad);
            }

            current++;
            onProgress?.Invoke(loc.PrimaryKey, (float)current / total);
        }

        _cache[label] = list;
    }

    // ─── Instanciation ───────────────────────────────────────────────────────
    private void TryInstantiate(object asset, bool dontDestroyOnLoad)
    {
        Debug.Log($"[AssetPreloader] Trying to instantiate asset of type {asset.GetType()}");
        if (asset is not GameObject prefab) return;

        var go = Object.Instantiate(prefab);
        go.name = prefab.name; // Supprime le "(Clone)"

        if (dontDestroyOnLoad)
            Object.DontDestroyOnLoad(go);

        _instantiated.Add(go);
        Debug.Log($"[AssetPreloader] Instantiated : {go.name}");
    }

    // ─── Accesseurs ──────────────────────────────────────────────────────────
    public T Get<T>(string label, int index = 0) where T : class
    {
        if (!_cache.TryGetValue(label, out var list)) return null;
        return list[index] as T;
    }

    public List<GameObject> GetInstantiated() => _instantiated;

    public void DestroyInstantiated()
    {
        foreach (var go in _instantiated)
            if (go != null) Object.Destroy(go);

        _instantiated.Clear();
    }
}