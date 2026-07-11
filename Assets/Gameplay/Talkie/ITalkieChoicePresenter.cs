using System;
using LightHouse.Core.Audio;

namespace LightHouse.Features.Talkie
{
    /// <summary>
    /// Contrat pour tout composant capable d'afficher une liste de TalkieChoice
    /// et de notifier la sélection du joueur. TalkieManager ne connaît que cette
    /// interface : on peut donc remplacer l'implémentation (boutons classiques,
    /// menu radial, sélection au clavier/manette...) sans toucher au moteur de
    /// dialogue, ce qui garde le système scalable côté UI.
    /// </summary>
    public interface ITalkieChoicePresenter
    {
        /// <summary>
        /// Affiche les choix. Doit appeler onSelected exactement une fois,
        /// avec le TalkieChoice choisi par le joueur.
        /// </summary>
        void Present(TalkieChoice[] choices, Action<TalkieChoice> onSelected);

        /// <summary>
        /// Cache l'UI de choix (fin de sélection ou annulation).
        /// </summary>
        void Hide();
    }
}
