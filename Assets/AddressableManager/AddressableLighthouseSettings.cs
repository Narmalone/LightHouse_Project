#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
#endif

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using AYellowpaper.SerializedCollections;
using UnityEngine.ResourceManagement.AsyncOperations;

[CreateAssetMenu(fileName = "New AddressableGroupSettings", menuName = "Addressable/Group Settings")]
public class AddressableLighthouseSettings : ScriptableObject
{
    public AddressableAssetSettings _accessToGameAddressableSettings;
    public List<string> Groups = new List<string>();
    public List<string> Labels = new List<string>();

    [Header("Mettre un nom de groupe ou label")] public string FindResourcesInGroup = "InteractableItems";
    [Header("Mettre un nom de groupe ou label")] public string FindResourcesInLabel = "items";

    [SerializedDictionary("Items Address", "Attached Items Type")]
    public SerializedDictionary<string, GameObject> AssetsItemsByGroup = new SerializedDictionary<string, GameObject>();
    public SerializedDictionary<string, List<GameObject>> ObjectsByLabels = new();
    public SerializedDictionary<string, GameObject> AssetsInAllGroupsExceptBuiltIn = new();

    /// <summary>
    /// Retrieves all Addressable labels and stores them in the Labels list.
    /// </summary>
    public void GetAddressableLabels()
    {
        Labels.Clear();

        // Access Addressable settings
        if (_accessToGameAddressableSettings == null)
        {
            Debug.LogError("Addressable settings not found!");
            return;
        }

        // Add all labels to the list
        foreach (string label in _accessToGameAddressableSettings.GetLabels())
        {
            Labels.Add(label);
        }
    }

    public void GetAddressableGroup()
    {
        Groups.Clear();

        // Access Addressable settings
        if (_accessToGameAddressableSettings == null)
        {
            Debug.LogError("Addressable settings not found!");
            return;
        }

        // Iterate through all groups and collect keys
        foreach (AddressableAssetGroup group in _accessToGameAddressableSettings.groups)
        {
            if (group == null) continue;
            Groups.Add(group.name);
        }

    }

    // Trier un dictionnaire par clé et conserver le résultat dans une nouvelle structure triée
    public SerializedDictionary<string, GameObject> GetSortedAssetsByKey()
    {
        var sortedDictionary = new SerializedDictionary<string, GameObject>();

        // Obtenir une liste triée des clés
        var sortedKeys = new List<string>(AssetsItemsByGroup.Keys);
        sortedKeys.Sort();

        // Reconstruire le dictionnaire trié
        foreach (var key in sortedKeys)
        {
            sortedDictionary.Add(key, AssetsItemsByGroup[key]);
        }

        return sortedDictionary;
    }

    /// <summary>
    /// Retrieves all Addressable keys and stores them in the Groups list.
    /// </summary>
    public void GetAddressableKeys()
    {
        Groups.Clear();

        // Access Addressable settings
        if (_accessToGameAddressableSettings == null)
        {
            Debug.LogError("Addressable settings not found!");
            return;
        }

        // Iterate through all groups and collect keys
        foreach (AddressableAssetGroup group in _accessToGameAddressableSettings.groups)
        {
            if (group == null || group.entries == null) continue;

            foreach (AddressableAssetEntry entry in group.entries)
            {
                Groups.Add(entry.address); // Use entry.address to get the key
            }
        }

        Groups.Sort();
    }

    public void LoadAssetsFromLabel()
    {
        if (_accessToGameAddressableSettings == null)
        {
            Debug.LogError("Addressable settings not found");
            return;
        }

        ObjectsByLabels = new SerializedDictionary<string, List<GameObject>>();
        
        foreach (string label in Labels)
        {
            Addressables.LoadAssetsAsync<GameObject>(label, (asset) =>
            {
                if (!ObjectsByLabels.ContainsKey(label))
                {
                    ObjectsByLabels.Add(label, new List<GameObject>());
                }

                ObjectsByLabels[label].Add(asset);

                // Vous pouvez ici instancier l'asset si nécessaire
                Debug.Log($"Asset {asset.name} loaded from label: {label}");
            }).Completed += (opHandle) =>
            {
                if (opHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    //Debug.Log($"Finished loading all assets for label: {label}");
                }
                else
                {
                    //Debug.LogError($"Failed to load assets for label: {label}");
                }
            };
        }
        
    }

    public void GetItemsAddressInSpecificGroup(string groupName)
    {
        if (_accessToGameAddressableSettings == null)
        {
            Debug.LogError("Addressable settings not found");
            return;
        }
        GetAddressableGroup();

        foreach (AddressableAssetEntry s in _accessToGameAddressableSettings.FindGroup(groupName).entries)
        {
            if (AssetsItemsByGroup.ContainsKey(s.address))
            {
                continue;
            }
            AssetsItemsByGroup.Add(s.address, s.MainAsset as GameObject);
        }

        AssetsItemsByGroup = GetSortedAssetsByKey();
    }

    public void GetItemsAddressInAllGroupsExceptBuiltIn()
    {
        if (_accessToGameAddressableSettings == null)
        {
            Debug.LogError("Addressable settings not found");
            return;
        }
        //GetAddressableGroup();


        //start to 1 to ignore the built in Group
        for (int i = 1; i < _accessToGameAddressableSettings.groups.Count; i++)
        {
            foreach (AddressableAssetEntry s in _accessToGameAddressableSettings.groups[i].entries)
            {
                if (AssetsInAllGroupsExceptBuiltIn.ContainsKey(s.address))
                {
                    continue;
                }
                AssetsInAllGroupsExceptBuiltIn.Add(s.address, s.MainAsset as GameObject);
            }
        }
    }

    public GameObject GetItemPrefabByAddress(string prefabKeyName)
    {
        if (!AssetsItemsByGroup.ContainsKey(prefabKeyName))
        {
            Debug.Log($"Aucun objet ne correspond à {prefabKeyName}");
        }

        return AssetsItemsByGroup[prefabKeyName];
    }

    public T GetItemPrefabByAddress<T>(string prefabKeyName) where T : Component
    {
        if (!AssetsItemsByGroup.ContainsKey(prefabKeyName))
        {
            Debug.Log($"Aucun objet ne correspond à {prefabKeyName}");
        }

        return AssetsItemsByGroup[prefabKeyName] as T;
    }

    public T InstantiatePrefabByAddress<T>(string prefabKeyName) where T : Component
    {
        if (!AssetsItemsByGroup.ContainsKey(prefabKeyName))
        {
            Debug.Log($"Aucun objet ne correspond à {prefabKeyName}");
        }
        
        return Instantiate(AssetsItemsByGroup[prefabKeyName]).GetComponent<T>();
    }

    public GameObject InstantiatePrefabByAddress(string prefabKeyName)
    {
        if (!AssetsItemsByGroup.ContainsKey(prefabKeyName))
        {
            Debug.Log($"Aucun objet ne correspond à {prefabKeyName}");
        }

        return Instantiate(AssetsItemsByGroup[prefabKeyName]);
    }

#if UNITY_EDITOR
    public void GenerateEnumKeys()
    {
        string enumName = "GeneratedKeys";
        string filePath = "Assets/AddressableManager/" + enumName + ".cs";

        if (!System.IO.Directory.Exists("Assets/AddressableManager"))
        {
            System.IO.Directory.CreateDirectory("Assets/AddressableManager");
        }

        List<string> keys = new List<string>(AssetsItemsByGroup.Keys);

        using (System.IO.StreamWriter writer = new System.IO.StreamWriter(filePath))
        {
            writer.WriteLine("public enum " + enumName);
            writer.WriteLine("{");

            for (int i = 0; i < keys.Count; i++)
            {
                string sanitizedKey = keys[i].Replace(" ", "_").Replace("-", "_");
                writer.WriteLine($"    {sanitizedKey} = {i},");
            }

            writer.WriteLine("}");
        }

        Debug.Log($"Enum {enumName} generated at {filePath}");
        AssetDatabase.Refresh(); // Refresh the AssetDatabase to include the new file
    }
#endif

}

#if UNITY_EDITOR
[CustomEditor(typeof(AddressableLighthouseSettings))]
public class AddressableGroupSettingsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AddressableLighthouseSettings settings = (AddressableLighthouseSettings)target;

        if (GUILayout.Button("Get All Groups Name"))
        {
            settings.GetAddressableGroup();
            EditorUtility.SetDirty(settings); // Mark the object as dirty to save changes
        }

        if (GUILayout.Button("Get All Labels Name"))
        {
            settings.GetAddressableLabels();
            EditorUtility.SetDirty(settings); // Mark the object as dirty to save changes
        } 
        
        if (GUILayout.Button("Get All Entries in Specific Group"))
        {
            settings.GetItemsAddressInSpecificGroup(settings.FindResourcesInGroup);
            EditorUtility.SetDirty(settings); // Mark the object as dirty to save changes
        }

        if (GUILayout.Button("Get All Assets by Label Names"))
        {
            settings.LoadAssetsFromLabel();
            EditorUtility.SetDirty(settings); // Mark the object as dirty to save changes
        }

        if (GUILayout.Button("Get All Assets in All Groups Except Built In"))
        {
            settings.GetItemsAddressInAllGroupsExceptBuiltIn();
            EditorUtility.SetDirty(settings); // Mark the object as dirty to save changes
        }

        if (GUILayout.Button("Generate Keys File Enum"))
        {
            settings.GenerateEnumKeys();
            EditorUtility.SetDirty(settings); // Mark the object as dirty to save changes
        }
    }
}
#endif
