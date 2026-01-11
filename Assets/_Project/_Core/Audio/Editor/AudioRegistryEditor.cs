using LightHouse.Core.Audio;

using UnityEngine;
using UnityEditor;

namespace LightHouse.Core.CustomEditors
{
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
}
