using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(OceanController))]
public class OceanSettingsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        OceanController currentInspectedSettings = (OceanController)target;

        if(GUILayout.Button("Override Ocean by Settings"))
        {
            
        }
    }
}
