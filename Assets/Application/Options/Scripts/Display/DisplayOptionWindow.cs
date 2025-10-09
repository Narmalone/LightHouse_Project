using LightHouse.Game.Options;
using LightHouse.Localization;
using TMPro;
using UnityEngine;

public class DisplayOptionsWindow : OptionWindowBase
{
    [SerializeField] private LocalizedStringDatabase_Options_Display _db;
    [Header("UI")]
    [SerializeField] private TMP_Dropdown _resolution;
    [SerializeField] private OptionEnum _displayMode;
    [SerializeField] private OptionEnum _monitors;
    [SerializeField] private TMP_Dropdown _refreshRate;
    [SerializeField] private TMP_Dropdown _frameRate;
    [SerializeField] private OptionToggle _vSync;

    // Settings
    private ResolutionSetting _resS;
    private DisplayModeSetting _modeS;
    private MonitorSetting _dispS;
    private RefreshRateSetting _rrS;
    private FrameRateLimitSetting _frS;
    private VSyncSetting _vsS;

    // Controllers
    private IOptionController _resC, _modeC, _monC, _rrC, _frC, _vsC;
    private IOptionSetting[] _settings;

    private void Start()
    {
        // settings
        _resS = new ResolutionSetting();
        _modeS = new DisplayModeSetting();
        _dispS = new MonitorSetting();
        _rrS = new RefreshRateSetting();
        _frS = new FrameRateLimitSetting();
        _vsS = new VSyncSetting();

        _settings = new IOptionSetting[] { _resS, _modeS, _vsS, _rrS, _frS, _dispS };

        // controllers (adapte avec tes implémentations existantes)
       /* _resC = new ResolutionDropdownController(_resolution, _resS, _db);
        _modeC = new DisplayModeController(_displayMode, _modeS);
        _monC = new MonitorsController(_monitors, _dispS, _db.Display);
        _rrC = new RefreshRateDropdownController(_refreshRate, _rrS, _db);
        _frC = new FrameRateDropdownController(_frameRate, _frS, _db);
        _vsC = new VSyncToggleController(_vSync, _vsS, _db);*/

        InitializeControllers();
    }

    public override void InitializeControllers()
    {
        /*_resC.Initialize(); _modeC.Initialize(); _monC.Initialize();
        _rrC.Initialize(); _frC.Initialize(); _vsC.Initialize();*/
    }

    public override void ApplySettings()
    {
        // Popup si changement d’écran
        if (_dispS.HasChanged() && confirmationPopupController != null)
        {
            confirmationPopupController.Show(
                confirmAction: () => ApplyAll(),
                cancelAction: () => _dispS.Revert(),
                timeOutAction: 15);
            return;
        }
        ApplyAll();
    }

    private void ApplyAll()
    {
        foreach (var s in _settings) if (s.HasChanged()) s.Apply();
    }

    public override void RevertSettings()
    {
        foreach (var s in _settings) if (s.HasChanged()) s.Revert();
        InitializeControllers();
    }

    public override bool HasChanges()
    {
        foreach (var s in _settings) if (s.HasChanged()) return true;
        return false;
    }
}
