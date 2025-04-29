using UnityEngine.Localization;
using UnityEngine.UIElements;

namespace LightHouse.Game.Options
{
    public class OptionsNavigationButton
    {
        private readonly Button button;
        private readonly OptionCategory targetCategory;
        private readonly OptionsMenuController controller;
        private LocalizedString LocalizedText;

        public Button Button => button;
        public OptionCategory TargetCategory => targetCategory;

        public OptionsNavigationButton(Button button, OptionCategory targetCategory, OptionsMenuController controller, LocalizedString languageText)
        {
            this.button = button;
            this.targetCategory = targetCategory;
            this.controller = controller;
            LocalizedText = languageText;

            this.button.clicked += OnClicked;
        }

        public void UpdateLocalizedText()
        {
            if (LocalizedText == null) return;
            button.text = LocalizedText.GetLocalizedString();
        }

        private void OnClicked()
        {
            controller.NavigateTo(targetCategory);
        }

        public void Dispose()
        {
            button.clicked -= OnClicked;
        }
    }
}
