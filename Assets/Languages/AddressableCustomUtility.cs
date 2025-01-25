using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
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
            Debug.Log($"{item.Key} = {item.Value.Result.name}");
        }

        Ready.Invoke();
        return operationDictionary;
    }
}