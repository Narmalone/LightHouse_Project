using UnityEngine.UIElements;
using UnityEngine;

namespace LightHouse.Game.Loading
{
    public class LoadingScreen : MonoBehaviour
    {
        #region FIELDS
        [SerializeField] private UIDocument _loadingScreenDocument;
        private VisualElement _root;
        private Label _progressText;
        private VisualElement _progressBar;
        private VisualElement _fill;

        #endregion

        #region UNITY LIFECYCLE
        void Awake()
        {
            _root = _loadingScreenDocument.rootVisualElement;
            _progressBar = _root.Q<VisualElement>("LoadingBar");
            _progressText = _progressBar.Q<Label>();
            _fill = _progressBar.Q<VisualElement>("ProgressFill");
        }
        #endregion

        #region Hide / Show
        public void Hide() => _root.style.display = DisplayStyle.None;
        public void Show() => _root.style.display = DisplayStyle.Flex;
        #endregion

        #region PROGRESS BAR FUNCS
        public void SetProgress(float value)
        {
            _fill.style.width = Length.Percent(value * 100f);
        }

        public void SetProgressName(string text)
        {
            _progressText.text = text;
        }
        #endregion
    }

}
