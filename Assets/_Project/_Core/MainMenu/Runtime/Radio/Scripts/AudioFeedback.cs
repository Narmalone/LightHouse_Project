using LightHouse.Core.Audio;
using LightHouse.Core.Services;
using UnityEngine;

namespace LightHouse.Core.Interaction.Feedback
{
    /// <summary>
    /// Gère les feedbacks audio liés aux interactions (hover, click).
    /// </summary>
    public class AudioFeedback : MonoBehaviour
    {
        #region ===== Settings =====

        [SerializeField] private SO_AudioCue _hoverSound;
        [SerializeField] private SO_AudioCue _clickSound;

        #endregion

        #region ===== State =====

        private InteractableBase _interactable;
        private bool _isEnabled = true;

        #endregion

        #region ===== Public API =====

        public void SetEnable(bool enable)
        {
            _isEnabled = enable;
        }

        public void Bind(InteractableBase interactable)
        {
            if (interactable == null) return;

            Unbind(); // 🔥 sécurité si rebinding

            _interactable = interactable;

            _interactable.OnHoverEnter += OnHoverEnter;
            _interactable.OnClickDown += OnClickDown;
        }

        #endregion

        #region ===== Unity Lifecycle =====

        private void OnDestroy()
        {
            Unbind();
        }

        #endregion

        #region ===== Binding =====

        private void Unbind()
        {
            if (_interactable == null) return;

            _interactable.OnHoverEnter -= OnHoverEnter;
            _interactable.OnClickDown -= OnClickDown;

            _interactable = null;
        }

        #endregion

        #region ===== Interaction =====

        private void OnHoverEnter()
        {
            if (!CanPlay()) return;

            PlaySound(_hoverSound);
        }

        private void OnClickDown()
        {
            if (!CanPlay()) return;

            PlaySound(_clickSound);
        }

        #endregion

        #region ===== Audio =====

        private bool CanPlay()
        {
            return _isEnabled && ServiceLocator.Audio != null;
        }

        private void PlaySound(SO_AudioCue cue)
        {
            if (cue == null) return;

            ServiceLocator.Audio.PlayAt(cue, transform.position);
        }

        #endregion
    }
}