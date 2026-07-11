using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

namespace LightHouse.Core.Audio
{
    /// <summary>
    /// Un choix proposé au joueur à la fin d'un dialogue Talkie.
    /// Purement data-driven : ajouter un nouveau choix, ou une nouvelle branche,
    /// ne nécessite aucun changement de code — juste une nouvelle entrée dans le
    /// tableau "Choices" d'une LocalizedDialogueAudio, et éventuellement de
    /// nouveaux assets LocalizedDialogueAudio pour la suite.
    /// </summary>
    [Serializable]
    public class TalkieChoice
    {
        [Tooltip("Texte du bouton affiché au joueur (localisé, même système que les sous-titres).")]
        [SerializeField] private LocalizedString label;

        [Tooltip("Dialogue(s) joué(s) juste après la sélection de ce choix. Peut être vide (fin de la conversation) ou pointer vers d'autres LocalizedDialogueAudio, qui peuvent eux-mêmes avoir leurs propres choix : la profondeur de l'arbre n'est pas limitée.")]
        [SerializeField] private LocalizedDialogueAudio[] nextDialogues;

        [Tooltip("Callback optionnel déclenché quand ce choix est sélectionné (ex: activer une quête, ouvrir une porte, changer une variable de jeu...).")]
        [SerializeField] private UnityEvent onChosen;

        public LocalizedString Label => label;
        public LocalizedDialogueAudio[] NextDialogues => nextDialogues;
        public UnityEvent OnChosen => onChosen;

        /// <summary>
        /// Position (0-based) de ce choix dans le tableau Choices de sa
        /// LocalizedDialogueAudio. Assignée automatiquement (voir
        /// LocalizedDialogueAudio.Choices) : c'est ce qu'un script appelant
        /// (tutoriel, quête...) peut lire pour savoir "quel choix" a été fait,
        /// sans dépendre de la référence exacte au TalkieChoice.
        /// La touche pressée par le joueur correspond à Index + 1.
        /// </summary>
        public int Index { get; private set; } = -1;

        internal void SetIndex(int index) => Index = index;

        /// <summary>
        /// Synchrone et safe (ne throw jamais), même pattern que LocalizedDialogueAudio.GetSubtitlesSafe().
        /// </summary>
        public string GetLabelSafe()
        {
            if (label == null)
                return string.Empty;

            if (label.TableReference.ReferenceType == TableReference.Type.Empty ||
                label.TableEntryReference.ReferenceType == TableEntryReference.Type.Empty)
                return string.Empty;

            try
            {
                return label.GetLocalizedString();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[TalkieChoice] GetLabelSafe failed: {e.Message}");
                return string.Empty;
            }
        }
    }
}
