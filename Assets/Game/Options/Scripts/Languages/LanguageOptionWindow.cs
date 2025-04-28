using System;
using LightHouse.Localization;
using UnityEngine;
using UnityEngine.UIElements;

namespace LightHouse.Game.Options
{
    public class LanguageOptionWindow : OptionWindowBase
    {
        private TextLanguagesDropdownController languageDropdownController;
        private LocalizedStringDatabase_Options_Languages languagesTextsDB;
        public LanguageOptionWindow(VisualElement rootElement, ConfirmationPopupController confirmationPopUp, LocalizedStringDatabase_Options_Languages languagesDB) : base(rootElement, confirmationPopUp)
        {
            languagesTextsDB = languagesDB;
            InitializeControllers();
        }

        public override void ApplySettings()
        {
            
        }

        public override bool HasChanges()
        {
            return false;
        }

        public override void InitializeControllers()
        {
            languageDropdownController = new TextLanguagesDropdownController(root.Q<DropdownField>("LanguageDropdown"), languagesTextsDB.Text_Languages);
            languageDropdownController.Initialize();
        }

        public override void RevertSettings()
        {
            
        }

        public void UpdateAllTextsLanguage()
        {
            languageDropdownController.UpdateLanguage();
        }
    }

}
