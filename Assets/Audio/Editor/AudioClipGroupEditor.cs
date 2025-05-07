using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BaseAudio_SO))]
public class AudioClipGroupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        BaseAudio_SO inspectedObject = (BaseAudio_SO)target;
        Debug.Log("slt");

        if (inspectedObject.displayName != inspectedObject.name)
        {
            inspectedObject.displayName = inspectedObject.name;
            Debug.Log("slt");
            EditorUtility.SetDirty(inspectedObject);
        }
    }
}
