using LightHouse.Game.Options;
using System;
using UnityEngine;
using UnityEngine.UI;

public class OptionMenuController : MonoBehaviour
{
    [SerializeField] private ConfirmationPopupController confirmationPopupController;
    [SerializeField] private Button applyCliqued;
    [SerializeField] private Button RevertButton;
    public VideoOptionsController videoOptionsController;
    public OptionsNavigationButton[] navigationButtons;

    private void Awake()
    {
        applyCliqued.onClick.AddListener(OnApplyCliqued);
        RevertButton.onClick.AddListener(OnRevertCliqued);
        SubscribeButtons();
    }

    private void SubscribeButtons()
    {
        foreach(var button in navigationButtons)
        {
            button.OnCliqued += Button_OnCliqued;
        }
    }

    private void UnsubscribeButtons()
    {
        foreach (var button in navigationButtons)
        {
            button.OnCliqued -= Button_OnCliqued;
        }
    }

    private void Button_OnCliqued(OptionsNavigationButton obj)
    {
        
    }

    private void OnValidate()
    {
        navigationButtons = GetComponentsInChildren<OptionsNavigationButton>();
    }

    private void OnRevertCliqued()
    {
        videoOptionsController.RevertAllSettings();
    }

    private void OnApplyCliqued()
    {
        confirmationPopupController.Show(
            confirmAction: () =>
            {
                videoOptionsController.ApplySettings();
            },
            cancelAction: () =>
            {
                videoOptionsController.RevertAllSettings();
            });
    }

    private void OnDestroy()
    {
        UnsubscribeButtons();
        applyCliqued.onClick.RemoveListener(OnApplyCliqued);
        RevertButton.onClick.RemoveListener(OnRevertCliqued);
    }
}
