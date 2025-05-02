using LightHouse.Inputs;
using LightHouse.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

namespace LightHouse.Game.Options
{
    #region ENUMS / CUSTOM LOCALIZED
    public enum UiPauseState
    {
        None,
        Menu,
        Pause,
        Options
    }

    public class LocalizedButton
    {
        public Button Button;
        public LocalizedString ButtonName;

        public LocalizedButton(Button target, LocalizedString buttonName)
        {
            Button = target;
            ButtonName = buttonName;
        }

        public void UpdateLanguageButtonLabel()
        {
            if (Button == null || ButtonName == null) return;
            Button.text = ButtonName.GetLocalizedString();
        }
    }
    #endregion

    public class PauseMenuController : MonoBehaviour
    {
        #region FIELDS
        [SerializeField] private UIDocument _pauseMenuDocument;
        [SerializeField] private OptionsMenuController _optionsMenuController;
        [SerializeField] private LocalizedStringDatabase_Menu_Pause _pauseMenuTextsDB;

        public UiPauseState State = UiPauseState.None;
        private VisualElement _rootMenu;
        private VisualElement _pauseMenuRoot;
        private VisualElement _optionsMenuRoot;

        private LocalizedButton _resumeButton;
        private LocalizedButton _optionsButton;
        private LocalizedButton _mainMenuButton;
        private LocalizedButton _quitButton;
        #endregion

        #region MONO CALLBACKS
        private void Awake()
        {
            _rootMenu = _pauseMenuDocument.rootVisualElement;
            _pauseMenuRoot = _rootMenu.Q<VisualElement>("PauseMenu");
            _optionsMenuRoot = _rootMenu.Q<VisualElement>("Options_Root");
            LocalizationSettings.SelectedLocaleChanged += LocalizationSettings_SelectedLocaleChanged;
            SearchAndInitializeButtons();
        }

        private void LateUpdate()
        {
            if (!InputManager.IsInitialized) return;
            if (InputManager.Player.PauseMenu.WasPerformedThisFrame())
            {
                if (State == UiPauseState.Pause)
                    PauseToNone();
                else if (State == UiPauseState.Options)
                    OptionsToPause();
                else
                    NoneToPause();
            }
        }

        private void OnDestroy()
        {
            UnregisterCallbacks();
            LocalizationSettings.SelectedLocaleChanged -= LocalizationSettings_SelectedLocaleChanged;
        }
        #endregion

        #region INIT
        private void SearchAndInitializeButtons()
        {
            _resumeButton = new LocalizedButton
            (
                _rootMenu.Q<Button>("ResumeButton"),
                _pauseMenuTextsDB.Resume
            );
            _optionsButton = new LocalizedButton
            (
                _rootMenu.Q<Button>("OptionsButton"),
                _pauseMenuTextsDB.Options
            );
            _mainMenuButton = new LocalizedButton
            (
                _rootMenu.Q<Button>("MainMenuButton"),
                _pauseMenuTextsDB.Main_Menu
            );
            _quitButton = new LocalizedButton
            (
                _rootMenu.Q<Button>("QuitButton"),
                _pauseMenuTextsDB.Quit_Game
            );

            RegisterCallbacks();
        }
        #endregion

        #region LOCALIZATION
        private void LocalizationSettings_SelectedLocaleChanged(Locale obj)
        {
            //_optionsMenuController.OnBackCliqued.UpdateLanguageButtonLabel();
            _resumeButton.UpdateLanguageButtonLabel();
            _optionsButton.UpdateLanguageButtonLabel();
            _mainMenuButton.UpdateLanguageButtonLabel();
            _quitButton.UpdateLanguageButtonLabel();
        }
        #endregion

        #region REGISTER / UNREGISTER
        private void RegisterCallbacks()
        {
            _optionsMenuController.OnBackClicked += OptionsMenuController_OnBackCliqued;
            _resumeButton.Button.clicked += Resume_Button_clicked;
            _mainMenuButton.Button.clicked += MainMenuCliqued;
            _optionsButton.Button.clicked += Options_Button_Clicked;
            _quitButton.Button.clicked += Quit_Button_clicked;
        }

        private void UnregisterCallbacks()
        {
            _optionsMenuController.OnBackClicked -= OptionsMenuController_OnBackCliqued;
            _resumeButton.Button.clicked -= Resume_Button_clicked;
            _mainMenuButton.Button.clicked -= MainMenuCliqued;
            _optionsButton.Button.clicked -= Options_Button_Clicked;
            _quitButton.Button.clicked -= Quit_Button_clicked;
        }
        #endregion

        #region BUTTONS CLICKED
        private void Resume_Button_clicked()
        {
            PauseToNone();
        }
        private void Options_Button_Clicked()
        {
            PauseToOptions();
        }
        private void OptionsMenuController_OnBackCliqued()
        {
            OptionsToPause();
        }

        private void MainMenuCliqued()
        {
            LightHouse.Game.BootStrap.BootStrap.Instance.ReturnToMenu();
        }

        private void Quit_Button_clicked()
        {
            Application.Quit();
        }

        #endregion

        #region SHOW / HIDE METHODS
        public void HideOptionsMenu()
        {
            _optionsMenuRoot.style.display = DisplayStyle.None;
            _optionsMenuController.ConfirmationPopupController.Hide();
        }

        public void ShowOptionsMenu()
        {
            _optionsMenuRoot.style.display = DisplayStyle.Flex;
        }

        public void ShowPauseMenu()
        {
            _pauseMenuRoot.style.display = DisplayStyle.Flex;
        }

        public void HidePauseMenu()
        {
            _pauseMenuRoot.style.display = DisplayStyle.None;
        }
        #endregion

        #region NAVIGATIONS
        public void PauseToNone()
        {
            HidePauseMenu();
            State = UiPauseState.None;
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
        }

        public void PauseToOptions()
        {
            State = UiPauseState.Options;
            HidePauseMenu();
            ShowOptionsMenu();
        }

        public void OptionsToPause()
        {
            State = UiPauseState.Pause;
            HideOptionsMenu();
            ShowPauseMenu();
        }

        public void NoneToPause()
        {
            State = UiPauseState.Pause;
            ShowPauseMenu();
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
        }
        #endregion
    }

}
