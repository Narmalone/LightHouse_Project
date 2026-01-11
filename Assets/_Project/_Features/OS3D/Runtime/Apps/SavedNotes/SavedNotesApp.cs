using LightHouse.Core.Services;
using LightHouse.Features.Computer.OS;
using System.Collections.Generic;
using UnityEngine;

namespace LightHouse.Features.Computer.NoteSystem
{
    /// <summary>
    /// Gère l'affichage et la gestion des notes sauvegardées via des raccourcis dans l'interface utilisateur.
    /// </summary>
    public class SavedNotesApp : ComputerApp
    {
        #region Inspector Fields

        [Header("Note UI Prefabs")]
        [Tooltip("Prefab utilisé pour instancier un raccourci de note.")]
        [SerializeField] private NotesShortcut _prefab;

        [Header("Parents")]
        [Tooltip("Parent transform qui contiendra tous les raccourcis de notes.")]
        [SerializeField] private Transform _notesContentParent;

        [Header("Note Data")]
        [Tooltip("Liste des notes actuellement chargées.")]
        [SerializeField] private List<NoteData> _notes;

        [Tooltip("Liste des raccourcis de notes instanciés.")]
        [SerializeField] private List<NotesShortcut> _savedNotes = new List<NotesShortcut>();

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// Souscription aux événements de sauvegarde/destruction des notes.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            NoteSaveSystem.OnNoteFileSaved += NoteSaveSystem_OnNoteFileSaved;
            NoteSaveSystem.OnNoteFileDestroyed += NoteSaveSystem_OnNoteFileDestroyed;
        }

        /// <summary>
        /// Charge les notes sauvegardées à la première frame.
        /// </summary>
        private void Start()
        {
            LoadAllSavedNotes();
        }

        /// <summary>
        /// Désinscription des événements à la destruction de l'objet.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            NoteSaveSystem.OnNoteFileSaved -= NoteSaveSystem_OnNoteFileSaved;
            NoteSaveSystem.OnNoteFileDestroyed -= NoteSaveSystem_OnNoteFileDestroyed;
        }

        /// <summary>
        /// Raccourcis de test : touche 0 pour sauvegarder une note aléatoire, touche 9 pour tout supprimer.
        /// </summary>
        private void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                SaveNote(new NoteData($"Day{Random.Range(0, 31)}", $"Je suis le con n°{Random.Range(0, 350)}"));
                Debug.Log("saving note");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                NoteSaveSystem.DestroyNoteFile();
                _notes.Clear();
            }
#endif
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Met à jour l’UI quand un fichier de notes est sauvegardé.
        /// </summary>
        private void NoteSaveSystem_OnNoteFileSaved()
        {
            for (int i = 0; i < _savedNotes.Count; i++)
            {
                _savedNotes[i].SetNoteDatas(_notes[i]);
            }
        }

        /// <summary>
        /// Nettoie l'UI et les données quand le fichier de notes est supprimé.
        /// </summary>
        private void NoteSaveSystem_OnNoteFileDestroyed()
        {
            foreach (NotesShortcut noteData in _savedNotes)
            {
                Destroy(noteData.gameObject);
            }
            _savedNotes.Clear();
            _notes.Clear();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Ajoute une note et la sauvegarde dans le système.
        /// </summary>
        public void SaveNote(NoteData note)
        {
            _notes.Add(note);
            NoteSaveSystem.SaveNotes(_notes);
        }

        /// <summary>
        /// Charge toutes les notes sauvegardées et instancie les raccourcis UI correspondants.
        /// </summary>
        public void LoadAllSavedNotes()
        {
            _notes = NoteSaveSystem.LoadNotes();

            foreach (var note in _notes)
            {
                var newNote = Instantiate(_prefab, _notesContentParent);
                newNote.Initialize(_os);
                newNote.SetNoteDatas(note);
                _savedNotes.Add(newNote);
            }
        }

        #endregion

        #region App Overrides

        public override void OnClose(bool playSound = true)
        {
            if (ServiceLocator.Audio != null && _onCloseSound && playSound)
                ServiceLocator.Audio.PlayAt(_onCloseSound, this.transform.position);
            Destroy(this.gameObject);
        }

        public override void OnMinimize() { }

        public override void OnOpen(bool playSound = true) 
        {
            if (ServiceLocator.Audio != null && _onOpenSound && playSound)
                ServiceLocator.Audio.PlayAt(_onOpenSound, this.transform.position);
        }

        #endregion
    }
}
