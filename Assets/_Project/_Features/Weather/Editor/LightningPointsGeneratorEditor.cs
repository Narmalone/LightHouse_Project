using LightHouse.Features.Weather.Lightnings;
using UnityEditor;
using UnityEngine;

namespace LightHouse.Core.CustomEditors
{
    [CustomEditor(typeof(LightningPointsGenerator))]
    public class LightningPointsGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var current = (LightningPointsGenerator)target;

            if (GUILayout.Button("Regenerate"))
            {
                current.Regenerate();
                EditorUtility.SetDirty(target);
            }
            if (GUILayout.Button("Set VFX Binder"))
            {
                current.AssignChildrenToVFXBinder_Reflection();
                EditorUtility.SetDirty(target);
            }
        }
    }

}
