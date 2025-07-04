using System.Collections.Generic;
using UnityEngine;

public class SavedNotesApp : ComputerApp
{
    public List<NoteData> Notes;
    private void Start()
    {
        LoadAllSavedNotes();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            SaveNote(new NoteData($"Day{Random.Range(0, 31)}", content: $"Je suis le con n°{Random.Range(0, 350)}"));
            Debug.Log("saving note");
        }
        else if (Input.GetKeyDown(KeyCode.R)) 
        {
            NoteSaveSystem.DestroyNoteFile();
            LoadAllSavedNotes();
        }
    }

    public void SaveNote(NoteData note)
    {
        Notes.Add(note);
        NoteSaveSystem.SaveNotes(Notes);
    }

    public override void OnClose()
    {
        Destroy(this.gameObject);
    }

    public override void OnMinimize()
    {
        
    }

    public override void OnOpen()
    {

    }

    public void LoadAllSavedNotes()
    {
        Notes = NoteSaveSystem.LoadNotes();
    }
}
