using LightHouse.Launcher;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LightHouse.CustomEditors
{
    public class ManifestManagerWindow : EditorWindow
    {
        [MenuItem("Tools/Manifest Manager")]
        public static void ShowWindow()
        {
            GetWindow<ManifestManagerWindow>("Manifest Manager Window");
        }

        private string buildFolder = "D:/Projects/LightHouseGame/Builds/Windows";

        void OnGUI()
        {
            GUILayout.Label("Génération de manifest", EditorStyles.boldLabel);

            buildFolder = EditorGUILayout.TextField("Dossier du build :", buildFolder);

            if (GUILayout.Button("Générer manifest.json"))
            {
                string output = Path.Combine(buildFolder, "manifest.json");
                try
                {
                    ManifestGenerator.Generate(buildFolder, output);
                    Debug.Log("Manifest généré avec succès.");
                }
                catch (Exception ex)
                {
                    Debug.LogError("Erreur lors de la génération du manifest : " + ex.Message);
                }
            }
        }

    }
}
