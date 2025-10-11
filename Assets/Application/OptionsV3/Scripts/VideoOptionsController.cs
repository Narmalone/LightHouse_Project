using LightHouse.Game.Options;
using LightHouse.Options.V3;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VideoOptionsController : OptionWindowBase
{
    public List<IOption> VideosOptions = new List<IOption>();
    public SimpleDisplayModeTogglerWithUI DisplayModeController;
    public MonitorController MonitorController;
    public ResolutionController ResolutionController;
    public RefreshRateController RefreshRateController;
    public FrameRateController FrameRateController;
    public VSyncController VSyncController;
    public QualityConfigController QualityController;

    private void Awake()
    {
        VideosOptions = GetComponentsInChildren<IOption>().ToList();
        FrameRateController.OnApplied += FrameRateController_OnApplied;
    }

    private void OnDestroy()
    {
        FrameRateController.OnApplied -= FrameRateController_OnApplied;
    }

    private void FrameRateController_OnApplied(int obj)
    {
        if(obj == -1)
        {
            VSyncController.OnFrameRateIllimited();
        }
        else
        {
            VSyncController.OnFrameRateDifferentThanIllimited();
        }
    }

    public override void ApplySettings()
    {
        DisplayModeController?.Apply();
        MonitorController?.Apply();
        ResolutionController?.Apply();
        RefreshRateController?.Apply();
        FrameRateController?.Apply();
        VSyncController?.Apply();
        QualityController?.Apply();
    }

    public override bool HasChanges()
    {
        foreach(IOption option in VideosOptions)
        {
            if (option.HasChanges()) return true;
        }
        return false;
    }

    public void RevertAllSettings()
    {
        foreach (var option in VideosOptions)
        {
            option.Revert();
        }
    }

    private void OnValidate()
    {
        VideosOptions = GetComponentsInChildren<IOption>().ToList();
    }

    public override void InitializeControllers()
    {
        
    }

    public override void RevertSettings()
    {
        DisplayModeController?.Revert();
        MonitorController?.Revert();
        ResolutionController?.Revert();
        RefreshRateController?.Revert();
        FrameRateController?.Revert();
        VSyncController?.Revert();
        QualityController?.Revert();
    }
}
