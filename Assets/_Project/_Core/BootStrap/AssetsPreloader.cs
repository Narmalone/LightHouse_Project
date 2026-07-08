using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AssetPreloader
{
    private class CachedAsset
    {
        public AsyncOperationHandle<object> Handle;
        public object Asset;
    }

    private class InstantiatedObject
    {
        public AsyncOperationHandle<GameObject> Handle;
        public GameObject Instance;
    }

    private readonly Dictionary<string, List<CachedAsset>> _cache = new();
    private readonly List<InstantiatedObject> _instantiated = new();

    #region PRELOAD

    public IEnumerator Preload(
        string label,
        bool instantiateOnLoad = false,
        bool dontDestroyOnLoad = false)
    {
        if (_cache.ContainsKey(label))
            yield break;

        var locationsHandle = Addressables.LoadResourceLocationsAsync(label);

        yield return locationsHandle;

        if (locationsHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"[AssetPreloader] Impossible de trouver le label '{label}'.");
            Addressables.Release(locationsHandle);
            yield break;
        }

        List<CachedAsset> assets = new();

        foreach (var location in locationsHandle.Result)
        {
            var handle = Addressables.LoadAssetAsync<object>(location);

            yield return handle;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[AssetPreloader] Impossible de charger {location.PrimaryKey}");
                continue;
            }

            assets.Add(new CachedAsset
            {
                Handle = handle,
                Asset = handle.Result
            });

            if (instantiateOnLoad && handle.Result is GameObject)
            {
                yield return Instantiate(location.PrimaryKey, dontDestroyOnLoad);
            }
        }

        _cache.Add(label, assets);

        Addressables.Release(locationsHandle);
    }

    public IEnumerator Preload(
        string label,
        Action<string, float> onProgress,
        bool instantiateOnLoad = false,
        bool dontDestroyOnLoad = false)
    {
        if (_cache.ContainsKey(label))
            yield break;

        var locationsHandle = Addressables.LoadResourceLocationsAsync(label);

        yield return locationsHandle;

        if (locationsHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"[AssetPreloader] Impossible de trouver le label '{label}'.");
            Addressables.Release(locationsHandle);
            yield break;
        }

        List<CachedAsset> assets = new();

        int total = locationsHandle.Result.Count;
        int current = 0;

        foreach (var location in locationsHandle.Result)
        {
            var handle = Addressables.LoadAssetAsync<object>(location);

            while (!handle.IsDone)
            {
                float globalProgress =
                    (current + handle.PercentComplete) / total;

                onProgress?.Invoke(location.PrimaryKey, globalProgress);

                yield return null;
            }

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                assets.Add(new CachedAsset
                {
                    Handle = handle,
                    Asset = handle.Result
                });

                if (instantiateOnLoad && handle.Result is GameObject)
                {
                    yield return Instantiate(location.PrimaryKey, dontDestroyOnLoad);
                }
            }

            current++;

            onProgress?.Invoke(location.PrimaryKey, (float)current / total);
        }

        _cache.Add(label, assets);

        Addressables.Release(locationsHandle);
    }

    #endregion

    #region INSTANTIATE

    public IEnumerator Instantiate(string address, bool dontDestroyOnLoad = false)
    {
        var handle = Addressables.InstantiateAsync(address);

        yield return handle;

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"[AssetPreloader] Impossible d'instancier {address}");
            yield break;
        }

        GameObject go = handle.Result;

        if (dontDestroyOnLoad)
            UnityEngine.Object.DontDestroyOnLoad(go);

        _instantiated.Add(new InstantiatedObject
        {
            Handle = handle,
            Instance = go
        });

        Debug.Log($"[AssetPreloader] Instantiated : {go.name}");
    }

    #endregion

    #region RELEASE

    public void Unload(string label)
    {
        if (!_cache.TryGetValue(label, out var assets))
            return;

        foreach (var asset in assets)
        {
            if (asset.Handle.IsValid())
                Addressables.Release(asset.Handle);
        }

        _cache.Remove(label);

        Debug.Log($"[AssetPreloader] Label '{label}' unloaded.");
    }

    public void UnloadAll()
    {
        foreach (var pair in _cache)
        {
            foreach (var asset in pair.Value)
            {
                if (asset.Handle.IsValid())
                    Addressables.Release(asset.Handle);
            }
        }

        _cache.Clear();

        Debug.Log("[AssetPreloader] Tous les assets ont été unload.");
    }

    public void DestroyInstantiated()
    {
        foreach (var obj in _instantiated)
        {
            if (obj.Handle.IsValid())
            {
                Addressables.ReleaseInstance(obj.Handle);
            }
        }

        _instantiated.Clear();
    }

    #endregion

    #region GET

    public T Get<T>(string label, int index = 0) where T : class
    {
        if (!_cache.TryGetValue(label, out var assets))
            return null;

        if (index < 0 || index >= assets.Count)
            return null;

        return assets[index].Asset as T;
    }

    public List<GameObject> GetInstantiated()
    {
        List<GameObject> list = new();

        foreach (var obj in _instantiated)
        {
            if (obj.Instance != null)
                list.Add(obj.Instance);
        }

        return list;
    }

    #endregion
}