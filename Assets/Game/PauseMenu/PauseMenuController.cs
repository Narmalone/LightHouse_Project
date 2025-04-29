using System;
using LightHouse.Game.Options;
using LightHouse.Inputs;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

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
}

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private UIDocument _pauseMenuDocument;
    public OptionsMenuController OptionsMenuController;

    public UiState State = UiState.None;

    private VisualElement rootMenu;
    private VisualElement _pauseMenuRoot;
    private VisualElement _optionsMenuRoot;

    private LightHouseButton _resumeButton;
    private LightHouseButton _optionsButton;
    private LightHouseButton _mainMenuButton;
    private LightHouseButton _quitButton;

    private void Awake()
    {
        rootMenu = _pauseMenuDocument.rootVisualElement;
        _pauseMenuRoot = rootMenu.Q<VisualElement>("PauseMenu");
        _optionsMenuRoot = rootMenu.Q<VisualElement>("OptionsContainer");
        OptionsMenuController.OnBackCliqued += OptionsMenuController_OnBackCliqued;

        _resumeButton = new LightHouseButton
        (
            rootMenu.Q<Button>("ResumeButton"), 
            null
        );
        _resumeButton.Button.clicked += Resume_Button_clicked;

        _optionsButton = new LightHouseButton
        (
            rootMenu.Q<Button>("OptionsButton"),
            null
        );

        _optionsButton.Button.clicked += Options_Button_Clicked;

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
        _quitButton.Button.clicked += Quit_Button_clicked;
    }

    private void Quit_Button_clicked()
    {
        Application.Quit();
    }

    private void OptionsMenuController_OnBackCliqued()
    {
        OptionsToPause();
    }

    private void Update()
    {
        if (InputManager.Player.PauseMenu.WasPerformedThisFrame())
        {
            if(State == UiState.Pause)
            {
                PauseToNone();
            }
            else if(State == UiState.Options)
            {
                OptionsToPause();
            }
            else
            {
                NoneToPause();
            }
            
        }
    }

    public void HideOptionsMenu()
    {
        _optionsMenuRoot.style.display = DisplayStyle.None;
        OptionsMenuController.confirmationPopupController.Hide();
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

    private void Options_Button_Clicked()
    {
        State = UiState.Options;
        ShowOptionsMenu();
        HidePauseMenu();
    }

    private void OnDestroy()
    {
        _resumeButton.Button.clicked -= Resume_Button_clicked;
        _optionsButton.Button.clicked -= Options_Button_Clicked;
        OptionsMenuController.OnBackCliqued -= OptionsMenuController_OnBackCliqued;
        _quitButton.Button.clicked -= Quit_Button_clicked;

    }

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

    private void Resume_Button_clicked()
    {
        PauseToNone();
    }
}
