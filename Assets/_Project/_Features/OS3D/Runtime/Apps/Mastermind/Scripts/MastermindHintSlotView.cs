using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Features.Computer.Mastermind
{
    /// <summary>
    /// Small hint bubble.
    /// </summary>
    public class MastermindHintSlotView : MonoBehaviour
    {
        #region Inspector

        [SerializeField]
        private Image _icon;

        [Header("Colors")]

        [SerializeField]
        private Color _emptyColor = Color.gray;

        [SerializeField]
        private Color _exactColor = Color.black;

        [SerializeField]
        private Color _partialColor = Color.white;

        #endregion

        #region Public API

        public void SetHint(MastermindHintType type)
        {
            switch (type)
            {
                case MastermindHintType.Empty:
                    _icon.color = _emptyColor;
                    break;

                case MastermindHintType.Exact:
                    _icon.color = _exactColor;
                    break;

                case MastermindHintType.Partial:
                    _icon.color = _partialColor;
                    break;
            }
        }

        #endregion
    }
}