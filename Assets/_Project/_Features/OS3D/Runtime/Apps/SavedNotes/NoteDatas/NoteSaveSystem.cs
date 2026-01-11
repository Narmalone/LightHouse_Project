using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LightHouse.Features.Computer.NoteSystem
{
    /// <summary>
    /// Gère la sauvegarde, le chargement et la suppression des notes utilisateur.
    /// </summary>
    public static class NoteSaveSystem
    {
        #region Events

        /// <summary>
        /// Événement déclenché après une sauvegarde réussie.
        /// </summary>
        public static event Action OnNoteFileSaved;

        /// <summary>
        /// Événement déclenché après la suppression du fichier de sauvegarde.
        /// </summary>
        public static event Action OnNoteFileDestroyed;

        #endregion

        #region Properties

        /// <summary>
        /// Chemin du fichier de sauvegarde des notes.
        /// </summary>
        private static string SavePath => Application.persistentDataPath + "/notes.json";

        #endregion

        #region Public API

        /// <summary>
        /// Sauvegarde une liste complète de notes dans un fichier JSON.
        /// </summary>
        public static void SaveNotes(List<NoteData> notes)
        {
            string json = JsonUtility.ToJson(new NoteListWrapper(notes), true);
            File.WriteAllText(SavePath, json);
            OnNoteFileSaved?.Invoke();
        }

        /// <summary>
        /// Sauvegarde ou met à jour une seule note. Identifie la note par son ID.
        /// </summary>
        public static void SaveNote(NoteData note)
        {
            List<NoteData> existingNotes = LoadNotes();

            // Nettoie toutes les anciennes entrées ayant le même ID (évite les doublons)
            existingNotes.RemoveAll(n => n.ID == note.ID);

            // Ajoute ou remplace la note
            existingNotes.Add(note);

            SaveNotes(existingNotes);
        }

        /// <summary>
        /// Vérifie si un titre de note existe déjà (hors note en cours d’édition).
        /// </summary>
        public static bool NoteTitleExists(string title, string currentNoteId = null)
        {
            var notes = LoadNotes();
            return notes.Exists(n => n.Title == title && n.ID != currentNoteId);
        }

        /// <summary>
        /// Charge toutes les notes depuis le fichier JSON.
        /// </summary>
        public static List<NoteData> LoadNotes()
        {
            if (!File.Exists(SavePath))
                return new List<NoteData>();

            string json = File.ReadAllText(SavePath);
            return JsonUtility.FromJson<NoteListWrapper>(json).Notes;
        }

        /// <summary>
        /// Supprime complètement le fichier de sauvegarde des notes.
        /// </summary>
        public static void DestroyNoteFile()
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
                OnNoteFileDestroyed?.Invoke();
                Debug.Log("destroying saved notes...");
            }
        }

        #endregion

        #region Data Wrappers

        /// <summary>
        /// Wrapper utilisé pour sérialiser/désérialiser une liste de notes avec JsonUtility.
        /// </summary>
        [Serializable]
        private class NoteListWrapper
        {
            public List<NoteData> Notes;

            public NoteListWrapper(List<NoteData> notes)
            {
                this.Notes = notes;
            }
        }

        #endregion
    }
}
