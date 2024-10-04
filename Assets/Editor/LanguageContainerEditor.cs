using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(LanguageContainer))]
public class LanguageContainerEditor : Editor
{
    bool showKeywordSelector = false;
    List<KeyWordLanguage> keyWords = new List<KeyWordLanguage>();

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        showKeywordSelector = EditorGUILayout.Foldout(showKeywordSelector, "Select From Preset Keyword");
        LanguageContainer container = (LanguageContainer)target;

        if (showKeywordSelector)
        {
            // Get the list of scriptable objects
            keyWords = GetKeywords();

            // Display a button for each scriptable object
            foreach (KeyWordLanguage keyword in keyWords)
            {
                if (GUILayout.Button(keyword.KeywordName))
                {
                    // Load languages from the selected keyword
                    LoadLanguagesFromKeyword(keyword);
                    container.AutomaticKeyword = keyword;
                    EditorUtility.SetDirty(container);
                }
            }
        }
    }

    // Helper method to get the list of scriptable objects
    List<KeyWordLanguage> GetKeywords()
    {
        List<KeyWordLanguage> keywords = new List<KeyWordLanguage>();
        string[] guids = AssetDatabase.FindAssets("t:KeyWordLanguage");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            KeyWordLanguage keyword = AssetDatabase.LoadAssetAtPath<KeyWordLanguage>(path);
            if (keyword != null)
            {
                keywords.Add(keyword);
            }
        }
        return keywords;
    }

    // Helper method to load languages from a keyword
    void LoadLanguagesFromKeyword(KeyWordLanguage keyword)
    {
        LanguageContainer languageContainer = (LanguageContainer)target;
        languageContainer.Languages.Clear();
        foreach (MultiLanguage language in keyword.Languages)
        {
            languageContainer.Languages.Add(language);
        }
    }
}