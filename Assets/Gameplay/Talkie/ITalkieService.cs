using System;
using LightHouse.Core.Audio;

namespace LightHouse.Features.Talkie
{
    public interface ITalkieService
    {
        void Enqueue(LocalizedDialogueAudio dialogueId);

        /// <summary>
        /// Enqueue un dialogue et récupère proprement le résultat s'il a des
        /// choix : onChoiceSelected est appelé une fois, avec le TalkieChoice
        /// sélectionné par le joueur (choice.Index donne sa position, 0-based).
        /// Si le dialogue n'a pas de choix, onChoiceSelected n'est jamais appelé.
        /// </summary>
        void Enqueue(LocalizedDialogueAudio dialogueId, Action<TalkieChoice> onChoiceSelected);
    }
}
