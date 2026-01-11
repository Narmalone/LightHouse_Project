using LightHouse.Core.Audio;
using LightHouse.Core.Services;
using LightHouse.Features.Computer.OS;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Features.Computer.NoteSystem
{
    /// <summary>
    /// Application permettant de lire, éditer et sauvegarder une note individuelle.
    /// </summary>
    public class NotesApp : ComputerApp
    {
        #region Inspector Fields

        [Header("Note Data")]
        [Tooltip("Contenu de la note en cours d’édition.")]
        [SerializeField] private NoteData _noteContent;

        [Header("UI References")]
        [Tooltip("Champ de saisie du titre de la note.")]
        [SerializeField] private TMP_InputField _titleInputField;

        [Tooltip("Champ de contenu de la note. Attention : le champ TMP ne supporte pas bien le multiline si une limite de ligne est fixée.")]
        [SerializeField] private TMP_InputField _contentInputField;

        [SerializeField] private AudioCue _keyboardCue;

        [Tooltip("Bouton pour sauvegarder la note.")]
        [SerializeField] private Button _saveNoteButton;

        [Header("Visual Feedback")]
        [Tooltip("Couleur par défaut de l'arrière-plan secondaire du bouton de sauvegarde.")]
        [SerializeField] private Color _saveNoteSecondaryBaseColor;

        [Tooltip("Arrière-plan secondaire du bouton de sauvegarde.")]
        [SerializeField] private Image _saveNoteSecondaryBackground;

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// Abonne les listeners aux champs UI.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            _saveNoteButton.onClick.AddListener(OnSaveNoteClicked);
            _titleInputField.onValueChanged.AddListener(OnTitleTextChanged);
            _contentInputField.onValueChanged.AddListener(OnContentTextChanged);
        }

        /// <summary>
        /// Désabonne les listeners pour éviter les fuites mémoire.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            _saveNoteButton.onClick.RemoveListener(OnSaveNoteClicked);
            _contentInputField.onValueChanged.RemoveListener(OnContentTextChanged);
        }

        #endregion

        #region Data Handling

        /// <summary>
        /// Initialise les champs avec les données d’une note existante.
        /// </summary>
        public void SetNoteDatas(NoteData data)
        {
            _noteContent = data;
            _titleInputField.text = data.Title;
            _contentInputField.text = data.Content;
        }

        /// <summary>
        /// Appelé lorsque le texte du titre change. Active ou désactive la sauvegarde si le nom existe.
        /// </summary>
        private void OnTitleTextChanged(string newTitle)
        {
            bool titleExists = NoteSaveSystem.NoteTitleExists(newTitle, _noteContent.ID);

            _saveNoteButton.interactable = !titleExists;
            _saveNoteSecondaryBackground.color = titleExists ? Color.grey : _saveNoteSecondaryBaseColor;

            if(ServiceLocator.Audio != null && _keyboardCue)
            {
                ServiceLocator.Audio.PlayAt(_keyboardCue, this.transform.position);
            }

            // Met à jour le titre seulement si valide
            if (!titleExists)
                _noteContent.Title = newTitle;
        }

        /// <summary>
        /// Met à jour le contenu de la note à chaque modification.
        /// </summary>
        private void OnContentTextChanged(string newContent)
        {
            _noteContent.Content = newContent;
            if (ServiceLocator.Audio != null && _keyboardCue)
            {
                ServiceLocator.Audio.PlayAt(_keyboardCue, this.transform.position);
            }
        }

        /// <summary>
        /// Sauvegarde la note courante via le système de persistance.
        /// </summary>
        private void OnSaveNoteClicked()
        {
            NoteSaveSystem.SaveNote(_noteContent);
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
