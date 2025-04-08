using LightHouse.Inventory;
using UnityEditor;
using UnityEngine;
using System.Linq;
using AYellowpaper.SerializedCollections;

[CustomEditor(typeof(ItemDatabase))]
public class ItemDatabaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ItemDatabase itemDatabase = (ItemDatabase)target;

        if (GUILayout.Button("Update All Keys in Prefabs"))
        {
            UpdateAllPrefabKeys(itemDatabase);
        }

        if (GUILayout.Button("Trier par ID croissant"))
        {
            SortItems(itemDatabase, ascending: true);
        }

        if (GUILayout.Button("Trier par ID décroissant"))
        {
            SortItems(itemDatabase, ascending: false);
        }

        EditorUtility.SetDirty(itemDatabase);
    }

    /// <summary>
    /// Sort the dictionnary inside ItemDatabase
    /// </summary>
    /// <param name="db"> the current inspected ItemDatabase </param>
    /// <param name="ascending"> Ascending or not</param>
    private void SortItems(ItemDatabase db, bool ascending)
    {
        var sortedItems = ascending
            ? db.items.OrderBy(kvp => kvp.Key)
            : db.items.OrderByDescending(kvp => kvp.Key);

        SerializedDictionary<ushort, GameObject> newDict = new SerializedDictionary<ushort, GameObject>();
        foreach (var kvp in sortedItems)
        {
            newDict.Add(kvp.Key, kvp.Value);
        }

        db.items = newDict;

        Debug.Log($"Items sorted by ID ({(ascending ? "croissant" : "décroissant")})");
    }

    /// <summary>
    /// Update all keys inside the prefabs of GameObjects
    /// </summary>
    /// <param name="db"> the current inspected ItemDatabase </param>
    private void UpdateAllPrefabKeys(ItemDatabase db)
    {
        foreach (var kvp in db.items)
        {
            GameObject go = kvp.Value;
            if (go == null)
            {
                Debug.LogWarning($"Item ID {kvp.Key} has a null GameObject.");
                continue;
            }

            string prefabPath = AssetDatabase.GetAssetPath(go);
            if (string.IsNullOrEmpty(prefabPath))
            {
                Debug.LogWarning($"Cannot find prefab path for {go.name}");
                continue;
            }

            GameObject prefabContents = PrefabUtility.LoadPrefabContents(prefabPath);
            IInventoryItem inventoryItem = prefabContents.GetComponentInChildren<IInventoryItem>();
            if (inventoryItem != null)
            {
                inventoryItem.GlobalItemID = kvp.Key;
                Debug.Log($"Set GlobalItemID of '{go.name}' to {kvp.Key}");
                PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
            }
            else
            {
                Debug.LogWarning($"Prefab '{go.name}' does not contain IInventoryItem.");
            }

            PrefabUtility.UnloadPrefabContents(prefabContents);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
