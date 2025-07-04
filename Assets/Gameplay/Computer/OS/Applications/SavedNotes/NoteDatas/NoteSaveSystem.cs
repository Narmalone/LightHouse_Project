using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class NoteSaveSystem
{
    private static string SavePath => Application.persistentDataPath + "/notes.json";

    public static void SaveNotes(List<NoteData> notes)
    {
        string json = JsonUtility.ToJson(new NoteListWrapper(notes), true);
        File.WriteAllText(SavePath, json);
    }

    public static List<NoteData> LoadNotes()
    {
        if (!File.Exists(SavePath))
            return new List<NoteData>();

        string json = File.ReadAllText(SavePath);
        return JsonUtility.FromJson<NoteListWrapper>(json).notes;
    }

    public static void DestroyNoteFile()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
        }
    }

    [System.Serializable]
    private class NoteListWrapper
    {
        public List<NoteData> notes;

        public NoteListWrapper(List<NoteData> notes)
        {
            this.notes = notes;
        }
    }
}
