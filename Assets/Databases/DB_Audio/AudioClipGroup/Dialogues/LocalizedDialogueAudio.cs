using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace LightHouse.Audio
{
    [CreateAssetMenu(fileName = "LocalizedDialogue_", menuName = "LightHouse/Audio/Localized Dialogue")]
    public class LocalizedDialogueAudio : ScriptableObject
    {
        public LocalizedAudioClip clip;
        public LocalizedString subtitle;
        public string currentSubtitle;
        public AudioClip currentAudio;

        public float volume = 1f;
        public bool loop = false;
        public float spatialBlend = 0f;

        public void Register()
        {
            clip.AssetChanged += Clip_AssetChanged;
            subtitle.StringChanged += Subtitle_StringChanged;
        }

        public void Unregister()
        {
            clip.AssetChanged -= Clip_AssetChanged;
            subtitle.StringChanged -= Subtitle_StringChanged;
        }

        private void Subtitle_StringChanged(string value)
        {
            Debug.Log(value);
            currentSubtitle = value;
        }

        private void Clip_AssetChanged(AudioClip value)
        {
            Debug.Log(value.name);
            currentAudio = value;
        }

    }


}
