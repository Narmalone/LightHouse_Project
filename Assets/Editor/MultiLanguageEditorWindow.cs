using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MultiLanguageEditorWindow : EditorWindow
{
    public string[] TabsNames;

    public string[] ComputerUnderTabs;

    [MenuItem("MultiLanguageSystem/Window")]
    public static void ShowExample()
    {
        MultiLanguageEditorWindow wnd = GetWindow<MultiLanguageEditorWindow>();
        wnd.titleContent = new GUIContent("MyEditorWindow");

    }

    public void CreateGUI()
    {
        
    }

    public void CreateHeader()
    {

    }
}
