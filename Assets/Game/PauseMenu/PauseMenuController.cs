using System;
using LightHouse.Inputs;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

namespace LightHouse.Game.Options
{
    #region ENUMS / CUSTOM LOCALIZED
    public enum UiState
    {
        None,
        Menu,
        Pause,
        Options
    }

    public class LightHouseButton
    {
        public Button Button;
        public LocalizedString ButtonName;

        public LightHouseButton(Button target, LocalizedString buttonName)
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
        [SerializeField] private UIDocument _pauseMenuDocument;
        [SerializeField] private OptionsMenuController _optionsMenuController;

        public UiState State = UiState.None;
        private VisualElement rootMenu;
        private VisualElement _pauseMenuRoot;
        private VisualElement _optionsMenuRoot;

        private LightHouseButton _resumeButton;
        private LightHouseButton _optionsButton;
        private LightHouseButton _mainMenuButton;
        private LightHouseButton _quitButton;

        #region MONO CALLBACKS
        private void Awake()
        {
            rootMenu = _pauseMenuDocument.rootVisualElement;
            _pauseMenuRoot = rootMenu.Q<VisualElement>("PauseMenu");
            _optionsMenuRoot = rootMenu.Q<VisualElement>("OptionsContainer");
            LocalizationSettings.SelectedLocaleChanged += LocalizationSettings_SelectedLocaleChanged;
            SearchAndInitializeButtons();
        }

        private void LocalizationSettings_SelectedLocaleChanged(Locale obj)
        {
            //_optionsMenuController.OnBackCliqued.UpdateLanguageButtonLabel();
            _resumeButton.UpdateLanguageButtonLabel();
            _optionsButton.UpdateLanguageButtonLabel();
            _mainMenuButton.UpdateLanguageButtonLabel();
            _quitButton.UpdateLanguageButtonLabel();
        }

        private void LateUpdate()
        {
            if (InputManager.Player.PauseMenu.WasPerformedThisFrame())
            {
                if (State == UiState.Pause)
                    PauseToNone();
                else if (State == UiState.Options)
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
            _resumeButton = new LightHouseButton
            (
                rootMenu.Q<Button>("ResumeButton"),
                null
            );
            _optionsButton = new LightHouseButton
            (
                rootMenu.Q<Button>("OptionsButton"),
                null
            );
            _mainMenuButton = new LightHouseButton
            (
                rootMenu.Q<Button>("MainMenuButton"),
                null
            );
            _quitButton = new LightHouseButton
            (
                rootMenu.Q<Button>("QuitButton"),
                null
            );

            RegisterCallbacks();
        }
        #endregion

        #region REGISTER / UNREGISTER
        private void RegisterCallbacks()
        {
            _optionsMenuController.OnBackCliqued += OptionsMenuController_OnBackCliqued;
            _resumeButton.Button.clicked += Resume_Button_clicked;
            _mainMenuButton.Button.clicked += MainMenuCliqued;
            _optionsButton.Button.clicked += Options_Button_Clicked;
            _quitButton.Button.clicked += Quit_Button_clicked;
        }

        private void UnregisterCallbacks()
        {
            _optionsMenuController.OnBackCliqued -= OptionsMenuController_OnBackCliqued;
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
            throw new NotImplementedException();
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
            _optionsMenuController.confirmationPopupController.Hide();
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
            State = UiState.None;
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
        }

        public void PauseToOptions()
        {
            State = UiState.Options;
            HidePauseMenu();
            ShowOptionsMenu();
        }

        public void OptionsToPause()
        {
            State = UiState.Pause;
            HideOptionsMenu();
            ShowPauseMenu();
        }

        public void NoneToPause()
        {
            State = UiState.Pause;
            ShowPauseMenu();
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
        }
        #endregion
    }

}
