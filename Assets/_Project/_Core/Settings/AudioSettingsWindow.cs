using LightHouse.Game.Options;
using UnityEngine;

public class AudioSettingsWindow : OptionWindowBase
{
    [SerializeField] private AudioSettingsConfig _config;
    [SerializeField] private AudioVolumeSlider _musicSlider;
    [SerializeField] private AudioVolumeSlider _sfxSlider;
    [SerializeField] private AudioVolumeSlider _voiceSlider;

    #region OptionWindowBase
    public override void ApplySettings()
    {
        throw new System.NotImplementedException();
    }

    public override bool HasChanges()
    {
        throw new System.NotImplementedException();
    }

    public override void InitializeControllers()
    {
        throw new System.NotImplementedException();
    }

    public override void RevertSettings()
    {
        throw new System.NotImplementedException();
    }

    #endregion

    private void Start()
    {
        if (!_musicSlider.HasKey())
        {
            _musicSlider.slider.SetValueWithoutNotify(_config.MusicBaseVolume);
            _musicSlider.Apply(_config.MusicBaseVolume);
        }
        else _musicSlider.Load();

        if (!_sfxSlider.HasKey())
        {
            _sfxSlider.slider.SetValueWithoutNotify(_config.SFXBaseVolume);
            _sfxSlider.Apply(_config.SFXBaseVolume);
        }
        else _sfxSlider.Load();

        if (!_voiceSlider.HasKey())
        {
            _voiceSlider.slider.SetValueWithoutNotify(_config.DialogsBaseVolume);
            _voiceSlider.Apply(_config.DialogsBaseVolume);
        }
        else _voiceSlider.Load();
    }
}
