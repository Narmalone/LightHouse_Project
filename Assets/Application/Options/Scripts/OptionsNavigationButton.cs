using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.Localization;
using UnityEngine.UIElements;

namespace LightHouse.Game.Options
{
    public class OptionsNavigationButton
    {
        private readonly Button _button;
        private readonly OptionCategory targetCategory;
        private readonly OptionsMenuController _controller;
        private LocalizedString LocalizedText;
        
        public Button Button => _button;
        public OptionCategory TargetCategory => targetCategory;

        public OptionsNavigationButton(Button button, OptionCategory targetCategory, OptionsMenuController controller, LocalizedString languageText)
        {
            this._button = button;
            this.targetCategory = targetCategory;
            this._controller = controller;
            LocalizedText = languageText;

            this._button.clicked += OnClicked;
        }

        public void SetSelected(bool selected)
        {
            bool isAlreadySelected = _button.ClassListContains("selected-nav-button");

            if (selected && !isAlreadySelected)
            {
                _button.AddToClassList("selected-nav-button");
                //Debug.Log($"Selected → {_button.name}");
            }
            else if (!selected && isAlreadySelected)
            {
                _button.RemoveFromClassList("selected-nav-button");
                //Debug.Log($"Deselected → {_button.name}");
            }
        }


        public void UpdateLocalizedText()
        {
            if (LocalizedText == null) return;
            _button.text = LocalizedText.GetLocalizedString();
        }

        private void OnClicked()
        {
            _controller.NavigateTo(targetCategory);
            _controller.HighlightSelectedButton(this, true); // ← ajout ici pour notifier le menu
        }

        public void Dispose()
        {
            _button.clicked -= OnClicked;
        }
    }
}
