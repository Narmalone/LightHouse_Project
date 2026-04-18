using System.Collections;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public static class LoadingUtils
{
    public static IEnumerator LoadScenesWithFeedback(
        string label,
        System.Action<string, float> onProgress)
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

            current++;
            onProgress?.Invoke(loc.PrimaryKey, (float)current / total);
        }
    }

    public static IEnumerator PreloadWithFeedback(
    string label,
    System.Action<string, float> onProgress)
    {
        var locationsHandle = Addressables.LoadResourceLocationsAsync(label);
        yield return locationsHandle;

        var locations = locationsHandle.Result;

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

            current++;
            onProgress?.Invoke(loc.PrimaryKey, (float)current / total);
        }
    }
}