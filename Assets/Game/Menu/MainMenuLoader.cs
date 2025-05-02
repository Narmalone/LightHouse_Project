using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;

public class MainMenuLoader : MonoBehaviour
{
    [SerializeField] private string[] labelsToLoad = { "UI_MainMenu_Extra", "UI_TutorialPanel" };

    IEnumerator Start()
    {
        foreach (var label in labelsToLoad)
        {
            yield return LoadLocalAssets(label);
        }
    }

    IEnumerator LoadLocalAssets(string label)
    {
        var handle = Addressables.LoadAssetsAsync<GameObject>(label, asset =>
        {
            Instantiate(asset, transform); // ou root de ta scène
        }, true);

        yield return handle;

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"Erreur lors du chargement des assets pour le label {label}");
        }
    }
}
