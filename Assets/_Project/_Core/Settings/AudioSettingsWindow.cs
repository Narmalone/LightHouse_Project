using UnityEngine;

namespace LightHouse.Core.Settings.Audio
{
    public class AudioSettingsWindow : OptionWindowBase
    {
        [SerializeField] private AudioSettingsConfig _config;
        [SerializeField] private AudioVolumeSlider _musicSlider;
        [SerializeField] private AudioVolumeSlider _sfxSlider;
        [SerializeField] private AudioVolumeSlider _voiceSlider;

        #region OptionWindowBase
        public override void ApplySettings()
        {
            
        }

        public override bool HasChanges()
        {
            return false;
        }

        public override void InitializeControllers()
        {
            
        }

        public override void RevertSettings()
        {
            
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

}
