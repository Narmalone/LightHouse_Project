using LightHouse.Core.Interaction;
using LightHouse.Core.Interaction.Feedback;
using UnityEngine;

namespace LightHouse.Features.Menu.Radio
{
    /// <summary>
    /// Relie le bouton ON/OFF de la radio avec son interactable et ses feedbacks.
    /// </summary>
    public class RadioOnOffController : MonoBehaviour
    {
        #region ===== Dependencies =====

        [SerializeField] private InteractableBase _interactable;
        [SerializeField] private ToggleButton _toggle;
        [SerializeField] private AudioFeedback _audioFeedback;

        #endregion

        #region ===== Public API =====

        public ToggleButton Toggle => _toggle;

        #endregion

        #region ===== Unity Lifecycle =====

        private void Awake()
        {
            Bind();
        }

        private void OnDestroy()
        {
            Unbind();
        }

        #endregion

        #region ===== Binding =====

        private void Bind()
        {
            if (_interactable == null) return;

            _toggle?.Bind(_interactable);
            _audioFeedback?.Bind(_interactable);
        }

        private void Unbind()
        {
            // 🔥 Important uniquement si tes composants supportent Unbind (ce que tu fais maintenant)
            _toggle?.Bind(null);
            _audioFeedback?.Bind(null);
        }

        #endregion
    }
}