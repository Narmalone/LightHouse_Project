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

    private void Awake()
    {
        applyCliqued.onClick.AddListener(OnApplyCliqued);
        RevertButton.onClick.AddListener(OnRevertCliqued);
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
                videoOptionsController.ApplyAllSettings();
            },
            cancelAction: () =>
            {
                videoOptionsController.RevertAllSettings();
            });
    }

    private void OnDestroy()
    {
        applyCliqued.onClick.RemoveListener(OnApplyCliqued);
        RevertButton.onClick.RemoveListener(OnRevertCliqued);
    }
}
