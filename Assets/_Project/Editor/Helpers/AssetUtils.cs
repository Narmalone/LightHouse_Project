using UnityEditor;
using UnityEngine;

namespace LightHouse.EditorTools.SuperGameManager
{
    public static class AssetUtils
    {
        public static T FindFirstAsset<T>() where T : UnityEngine.Object
        {
            // IMPORTANT: le filtre doit être "t:TypeName"
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            if (guids == null || guids.Length == 0) return null;

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        public static T CreateScriptableAsset<T>(string folder, string fileName) where T : ScriptableObject
        {
            EnsureFolder(folder);

            var asset = ScriptableObject.CreateInstance<T>();
            var assetPath = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{fileName}.asset");

            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.SetDirty(asset);
            Selection.activeObject = asset;

            return asset;
        }

        public static void EnsureFolder(string folder)
        {
            // folder ex: "Assets/Configs/Time"
            if (AssetDatabase.IsValidFolder(folder)) return;

            var parts = folder.Split('/');
            if (parts.Length == 0 || parts[0] != "Assets") return;

            string current = "Assets";
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
