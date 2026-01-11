using LightHouse.Core.Utilities;
using UnityEditor;
using UnityEngine;

namespace LightHouse.Core.CustomEditors
{
    [CustomEditor(typeof(CircularColliderRing))]
    public class CircularColliderRingEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            CircularColliderRing ring = (CircularColliderRing)target;

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Regenerate"))
            {
                Undo.RecordObject(ring, "Regenerate Colliders");
                ring.GenerateColliders();
            }

            if (GUILayout.Button("Clear"))
            {
                Undo.RecordObject(ring, "Clear Colliders");
                ring.ClearColliders();
            }

            EditorGUILayout.EndHorizontal();
        }
    }

}
