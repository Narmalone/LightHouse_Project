using LightHouse.Game.Computer.OS;
using TMPro;
using UnityEngine;

namespace LightHouse.Game.Computer.NoteSystem
{
    /// <summary>
    /// Contrôleur de raccourci de note affiché dans l’interface principale.
    /// Permet de lancer l’application de note associée et y injecter les données.
    /// </summary>
    public class NotesShortcut : ShortCutController
    {
        #region Fields

        /// <summary>
        /// Données de la note liée à ce raccourci.
        /// </summary>
        protected NoteData _note;

        [Header("UI Reference")]
        [Tooltip("Référence vers le champ texte affichant le titre de la note.")]
        [SerializeField] private TextMeshProUGUI _noteTitleText;

        #endregion

        #region Public API

        /// <summary>
        /// Associe les données de la note au raccourci, et met à jour l'affichage.
        /// </summary>
        /// <param name="data">Les données de la note.</param>
        public void SetNoteDatas(NoteData data)
        {
            _note = data;
            _noteTitleText.text = data.Title;
        }

        #endregion

        #region Execution

        /// <summary>
        /// Exécute l'ouverture de l'application de note associée à ce raccourci.
        /// </summary>
        public override void OnExecute()
        {
            base.OnExecute();
            _currentInstance.RectTransform.anchoredPosition = GetRandomOffsetWindow();

            if (_currentInstance is NotesApp notesApp)
            {
                notesApp.SetNoteDatas(_note);
            }
        }

        #endregion
    }
}
