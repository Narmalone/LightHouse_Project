using System;
using UnityEngine;

namespace LightHouse.Core.Interaction
{
    /// <summary>
    /// Base générique pour tout objet interactable (hover + click).
    /// Sert de pont entre le système d'interaction et les événements gameplay.
    /// </summary>
    public class InteractableBase : MonoBehaviour,
        IClickable,
        IClickableUp,
        IRaycastEnter,
        IRaycastExit
    {
        #region ===== Events =====

        public event Action OnHoverEnter;
        public event Action OnHoverExit;

        public event Action OnClickDown;
        public event Action OnClickUp;

        #endregion

        #region ===== Raycast =====

        public void OnRaycastEnter()
        {
            OnHoverEnter?.Invoke();
        }

        public void OnRaycastExit()
        {
            OnHoverExit?.Invoke();
        }

        #endregion

        #region ===== Click =====

        public void OnClicked()
        {
            OnClickDown?.Invoke();
        }

        public void OnClickReleased()
        {
            OnClickUp?.Invoke();
        }

        #endregion
    }
}