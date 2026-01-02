using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VectorPath))]
public class VectorPathEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        VectorPath path = (VectorPath)target;

        if (GUILayout.Button("Générer un nouveau chemin"))
        {
            GenerateNewPath(path);
            EditorUtility.SetDirty(path);
        }
    }

    private void GenerateNewPath(VectorPath path)
    {
        path.GenerateNewPath();
    }
}
