using System;
using TMPro;
using UnityEngine;

namespace LightHouse.Features.Computer
{
    public class InfoPanelMenuController :
        MonoBehaviour
    {

        public event Action<bool> OnVisibleChanged;

        #region Inspector

        [SerializeField] private TextMeshProUGUI _titleText;

        [SerializeField]
        private RectTransform
            _infosContainer;

        [SerializeField]
        private TextMeshProUGUI
            _textInfosPrefab;

        [SerializeField]
        private CanvasGroup
            _canvasGroup;

        #endregion

        #region Fields

        private bool _isVisible;

        #endregion

        public bool IsVisible => _isVisible;

        #region Unity Lifecycle

        private void Start()
        {
            Hide();
        }

        #endregion

        #region Public API

        public void UpdateTitleText(string text)
        {
            _titleText.text = text;   
        }

        public void Initialize(
            string[] configTexts)
        {
            ClearInfos();

            for (int i = 0;
                 i < configTexts.Length;
                 i++)
            {
                TextMeshProUGUI infoText =
                    Instantiate(
                        _textInfosPrefab,
                        _infosContainer);

                infoText.text =
                    configTexts[i];
            }

            if(!_isVisible)
                Show();
        }

        public void Show()
        {
            _canvasGroup.alpha = 1f;

            _canvasGroup.interactable =
                true;

            _canvasGroup.blocksRaycasts =
                true;

            OnVisibleChanged?.Invoke(true);
            _isVisible = true;
        }

        public void Hide()
        {
            _canvasGroup.alpha = 0f;

            _canvasGroup.interactable =
                false;

            _canvasGroup.blocksRaycasts =
                false;

            OnVisibleChanged?.Invoke(false);
            _isVisible = false;
        }

        #endregion

        #region Internal

        private void ClearInfos()
        {
            for (int i =
                 _infosContainer.childCount - 1;
                 i >= 0;
                 i--)
            {
                Destroy(
                    _infosContainer
                        .GetChild(i)
                        .gameObject);
            }
        }

        #endregion
    }
}