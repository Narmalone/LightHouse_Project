using LightHouse.Core.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[Serializable]
public class BootstrapEntry
{
    [AddressableLabelSelector] public string label; // Addressable key
}

[System.Serializable]
public class BootstrapInstaller
{
    [SerializeField] private List<GameObject> _instances = new();

    public IEnumerator Install(List<BootstrapEntry> entries)
    {
        foreach (var entry in entries)
        {
            // 🔥 1. Récupérer tous les assets du label
            var locationsHandle = Addressables.LoadResourceLocationsAsync(entry.label, typeof(GameObject));
            yield return locationsHandle;

            if (locationsHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[Bootstrap] Failed to get locations for label {entry.label}");
                continue;
            }

            var locations = locationsHandle.Result;

            // 🔥 2. Instancier chaque asset
            foreach (var loc in locations)
            {
                var handle = Addressables.InstantiateAsync(loc);
                yield return handle;

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogError($"[Bootstrap] Failed to instantiate {loc.PrimaryKey}");
                    continue;
                }

                var instance = handle.Result;

                _instances.Add(instance);

                Debug.Log($"[Bootstrap] Instantiated: {loc.PrimaryKey}");
            }
        }
    }
}