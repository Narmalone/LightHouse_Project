using UnityEditor;
using UnityEngine;

namespace LightHouse.Game.Computer.NoteSystem.Editor
{
    /// <summary>
    /// Custom editor pour SavedNotesApp avec un bouton pour supprimer le fichier de sauvegarde.
    /// </summary>
    [CustomEditor(typeof(SavedNotesApp))]
    public class SavedNotesAppEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // Dessine l'inspecteur par défaut
            base.OnInspectorGUI();

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Note Debug Tools", EditorStyles.boldLabel);

            if (GUILayout.Button("Ajouter un fichier de note aléatoire"))
            {
                if (EditorUtility.DisplayDialog("Confirmer la création d'un fichier ?",
                    "Vous allez ajouter une note dans un fichier dans le AppData / DefaultCompany / LightHouse / note.json ?",
                    "Oui", "Annuler"))
                {
                    NoteSaveSystem.SaveNote(new NoteData($"Day_{Random.Range(0, 31) + "_" + $"{Random.Range(0, 24) + "_" + Random.Range(0, 60)}"}", $"Je suis le gardien du phare n°{Random.Range(0, 350)}"));
                }
            }
            else if (GUILayout.Button("📂 Ouvrir le dossier de sauvegarde"))
            {
                EditorUtility.RevealInFinder(Application.persistentDataPath);
            }
            else if (GUILayout.Button("🗑️ Supprimer le fichier de notes"))
            {
                if (EditorUtility.DisplayDialog("Confirmer la suppression",
                    "Es-tu sûr de vouloir supprimer le fichier de sauvegarde des notes ?",
                    "Oui", "Annuler"))
                {
                    NoteSaveSystem.DestroyNoteFile();
                }
            }
            
        }
    }
}
