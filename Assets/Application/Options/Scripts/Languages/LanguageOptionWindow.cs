using LightHouse.Localization;
using TMPro;
using UnityEngine;

namespace LightHouse.Game.Options
{
    public class LanguageOptionWindow : OptionWindowBase
    {
        private TextLanguagesDropdownController languageDropdownController;
        private LocalizedStringDatabase_Options_Languages languagesTextsDB;
        private TMP_Dropdown _languageDropdown;

       /* public LanguageOptionWindow(CanvasGroup rootElement, TMP_Dropdown languageDropdown, ConfirmationPopupController confirmationPopUp, LocalizedStringDatabase_Options_Languages languagesDB) : base(rootElement, confirmationPopUp)
        {
            languagesTextsDB = languagesDB;
            _languageDropdown = languageDropdown;
            InitializeControllers();
        }*/

        public override void ApplySettings()
        {
            foreach(var option in optionSettings)
            {
                option.Apply();
            }
        }

        public override bool HasChanges()
        {
            foreach (IOptionSetting setting in optionSettings)
            {
                if (setting.HasChanged()) return true;
            }
            return false;
        }

        public override void InitializeControllers()
        {
            languageDropdownController = new TextLanguagesDropdownController(_languageDropdown, languagesTextsDB.Text_Languages);
            languageDropdownController.Initialize();

            optionSettings = new IOptionSetting[1];
            optionSettings[0] = languageDropdownController.Setting;
        }

        public override void RevertSettings()
        {
            languageDropdownController.Revert();
        }

        public void UpdateAllTextsLanguage()
        {
            languageDropdownController.UpdateLanguage();
        }
    }

}
