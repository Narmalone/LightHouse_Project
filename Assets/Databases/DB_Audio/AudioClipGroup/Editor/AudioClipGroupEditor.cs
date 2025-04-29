using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AudioClipGroup))]
public class AudioClipGroupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        AudioClipGroup inspectedObject = (AudioClipGroup)target;
        Debug.Log("slt");

        if (inspectedObject.displayName != inspectedObject.name)
        {
            inspectedObject.displayName = inspectedObject.name;
            Debug.Log("slt");
            EditorUtility.SetDirty(inspectedObject);
        }
    }
}
