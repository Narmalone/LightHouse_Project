using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(AudioRegistry))]
public class AudioRegistryEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        AudioRegistry asset = (AudioRegistry)target;
        if (GUILayout.Button("Refresh"))
        {
            asset.RegenerateDatas();
            EditorUtility.SetDirty(asset);
        }
    }
}
