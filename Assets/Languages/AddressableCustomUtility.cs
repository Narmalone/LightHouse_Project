using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public static class AddressableCustomUtility
{
    public static async Task<Dictionary<string, AsyncOperationHandle<GameObject>>> LoadAndAssociateResultWithKey(List<string> keys, Action Ready)
    {
        Dictionary<string, AsyncOperationHandle<GameObject>> operationDictionary = new Dictionary<string, AsyncOperationHandle<GameObject>>();

        AsyncOperationHandle<IList<IResourceLocation>> locations =
            Addressables.LoadResourceLocationsAsync(keys, Addressables.MergeMode.Union, typeof(GameObject));

        IList<IResourceLocation> locationResults = await locations.Task;

        var loadOps = new List<AsyncOperationHandle>(locationResults.Count);

        foreach (IResourceLocation location in locationResults)
        {
            AsyncOperationHandle<GameObject> handle =
                Addressables.LoadAssetAsync<GameObject>(location);
            handle.Completed += obj => operationDictionary.Add(location.PrimaryKey, obj);
            loadOps.Add(handle);
        }

        await Addressables.ResourceManager.CreateGenericGroupOperation(loadOps, true).Task;

        foreach (var item in operationDictionary)
        {
            //Debug.Log($"{item.Key} = {item.Value.Result.name}");
        }

        Ready.Invoke();
        return operationDictionary;
    }

    /// <summary>
    /// Coroutine pour charger une ressource addressable de type T.
    /// </summary>
    /// <typeparam name="T">Le type du composant attendu.</typeparam>
    /// <param name="key">La clé ou l'adresse de la ressource dans Addressables.</param>
    /// <param name="onComplete">Callback appelé avec le résultat une fois chargé.</param>
    /// <returns>Une IEnumerator pour la coroutine.</returns>
    public static IEnumerator LoadAddressableCoroutine<T>(string key, System.Action<T> onComplete) where T : Component
    {
        // Charger l'asset en tant que GameObject
        AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(key);

        // Attendre la fin du chargement
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject loadedObject = handle.Result;

            // Vérifier si le GameObject contient le composant demandé
            T component = loadedObject.GetComponent<T>();
            if (component != null)
            {
                onComplete?.Invoke(component);
            }
            else
            {
                Debug.LogError($"GameObject loaded with key '{key}' does not contain component of type {typeof(T)}.");
                onComplete?.Invoke(null);
            }
        }
        else
        {
            Debug.LogError($"Failed to load Addressable with key '{key}'.");
            onComplete?.Invoke(null);
        }

        // Libérer le handle
        Addressables.Release(handle);
    }

    /// <summary>
    /// Coroutine pour charger et instancier un asset Addressable contenant un composant de type T.
    /// </summary>
    /// <typeparam name="T">Le type du composant attendu (doit être un Component).</typeparam>
    /// <param name="key">La clé Addressable pour identifier l'asset.</param>
    /// <param name="parent">Parent optionnel pour l'objet instancié.</param>
    /// <param name="onComplete">Callback appelé avec le composant instancié ou null en cas d'échec.</param>
    /// <returns>Une IEnumerator pour exécuter cette méthode comme coroutine.</returns>
    public static IEnumerator InstantiateAddressableCoroutine<T>(string key, Transform parent = null, System.Action<T> onComplete = null) where T : Component
    {
        // Démarre le chargement et l'instantiation de l'asset
        AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(key, parent);

        // Attend la fin de l'opération
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject instantiatedObject = handle.Result;

            // Récupère le composant T attaché à l'objet instancié
            T component = instantiatedObject.GetComponent<T>();
            if (component != null)
            {
                onComplete?.Invoke(component);
                yield break;
            }

            Debug.LogError($"Le GameObject instancié ne contient pas de composant de type {typeof(T).Name}.");
        }
        else
        {
            Debug.LogError($"Échec de l'instantiation de l'asset avec la clé: {key}. Erreur: {handle.OperationException?.Message}");
        }

        // En cas d'erreur, renvoie null
        onComplete?.Invoke(null);
    }

    public static IEnumerator LoadAssetRoutine(string key, Action<AsyncOperationHandle<GameObject>> result)
    {
        AsyncOperationHandle<GameObject> loadOp = Addressables.LoadAssetAsync<GameObject>(key);
        yield return loadOp;
        if (loadOp.Status == AsyncOperationStatus.Succeeded)
        {
            var op = Addressables.InstantiateAsync(key);
            if (op.IsDone) // <--- this will always be true.  A preloaded asset will instantiate synchronously. 
            {
                result?.Invoke(op);
            }
        }
    }


    /// <summary>
    /// Loads all assets with a specific label and returns a dictionary of their keys and objects using a coroutine.
    /// </summary>
    /// <param name="label">The Addressable label.</param>
    /// <param name="onComplete">Callback invoked when loading is complete with the dictionary of loaded assets.</param>
    /// <returns>An IEnumerator for coroutine execution.</returns>
    public static IEnumerator LoadAllWithLabelCoroutine(string label, Action<Dictionary<string, GameObject>> onComplete)
    {
        var operationDictionary = new Dictionary<string, GameObject>();

        // Load resource locations by label
        AsyncOperationHandle<IList<UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation>> locations =
            Addressables.LoadResourceLocationsAsync(label);

        yield return locations;

        if (locations.Status == AsyncOperationStatus.Succeeded)
        {
            foreach (var location in locations.Result)
            {
                // Load each asset
                AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(location.PrimaryKey);
                yield return handle;

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    operationDictionary[location.PrimaryKey] = handle.Result;
                }
                else
                {
                    Debug.LogError($"Failed to load asset with key: {location.PrimaryKey}");
                }
            }

            onComplete?.Invoke(operationDictionary);
        }
        else
        {
            Debug.LogError($"Failed to load locations with label: {label}");
            onComplete?.Invoke(null);
        }
    }
}