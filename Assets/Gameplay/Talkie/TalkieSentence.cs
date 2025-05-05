using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace LightHouse.Game.Talkie
{
    [CreateAssetMenu(fileName = "Talkie_Sentence_", menuName = "LightHouse/Talkie/New Dialogue")]
    public class TalkieSentence : ScriptableObject
    {
        public LocalizedAudioClip AudioClip;
        public LocalizedString Sentence;
        public float Delay = 0.0f;
        public float AdditionnalTimeAtEndOfDialogue = 0.3f;

        [SerializeField] private AudioClip _currentAudioToPlay;

        public void Register()
        {
            LocalizationSettings.SelectedLocaleChanged += LocalizationSettings_SelectedLocaleChanged;
            AudioClip.AssetChanged += AudioClip_AssetChanged;
        }

        public void Unregister()
        {
            LocalizationSettings.SelectedLocaleChanged -= LocalizationSettings_SelectedLocaleChanged;
            AudioClip.AssetChanged -= AudioClip_AssetChanged;
        }

        private void LocalizationSettings_SelectedLocaleChanged(Locale obj)
        {
            
        }

        private void AudioClip_AssetChanged(AudioClip value)
        {
            _currentAudioToPlay = AudioClip.LoadAsset();
            Debug.Log(_currentAudioToPlay.name);
        }
    }

}
