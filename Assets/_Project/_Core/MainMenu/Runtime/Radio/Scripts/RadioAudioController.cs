using LightHouse.Core.Audio;
using LightHouse.Core.Services;
using UnityEngine;

namespace LightHouse.Features.Menu.Radio
{
    /// <summary>
    /// Gère l'audio de la radio (bruit + voix) en fonction de la fréquence.
    /// </summary>
    public class RadioAudioController : MonoBehaviour
    {
        #region ===== Dependencies =====

        [SerializeField] private RadioFrequencyController _frequencyController;

        #endregion

        #region ===== Settings =====

        [Header("Audio")]
        [SerializeField] private SO_AudioCue _noiseCue;
        [SerializeField] private SO_AudioCue _voiceCue;

        [Header("Tuning")]
        [SerializeField] private float _maxDistance = 2f;

        #endregion

        #region ===== State =====

        private IAudioHandle _noiseHandle;
        private IAudioHandle _voiceHandle;

        #endregion

        #region ===== Unity Lifecycle =====

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
            StopAudio(); // 🔥 sécurité
        }

        #endregion

        #region ===== Binding =====

        private void Subscribe()
        {
            if (_frequencyController == null) return;

            _frequencyController.OnFrequencyChanged += UpdateAudio;
        }

        private void Unsubscribe()
        {
            if (_frequencyController == null) return;

            _frequencyController.OnFrequencyChanged -= UpdateAudio;
        }

        #endregion

        #region ===== Public API =====

        public void PlayAudio()
        {
            if (ServiceLocator.Audio == null) return;

            _noiseHandle = ServiceLocator.Audio.PlayAt(_noiseCue, transform.position);
            _voiceHandle = ServiceLocator.Audio.PlayAt(_voiceCue, transform.position);
        }

        public void StopAudio()
        {
            _noiseHandle?.Stop();
            _voiceHandle?.Stop();

            _noiseHandle = null;
            _voiceHandle = null;
        }

        #endregion

        #region ===== Audio Logic =====

        private void UpdateAudio(float currentFrequency, float targetFrequency)
        {
            float distance = Mathf.Abs(currentFrequency - targetFrequency);

            float t = ComputeBlend(distance);

            ApplyVolumes(t);
        }

        private float ComputeBlend(float distance)
        {
            return Mathf.InverseLerp(_maxDistance, 0f, distance);
        }

        private void ApplyVolumes(float t)
        {
            // Noise ↓ quand on est proche
            SetVolume(_noiseHandle, 1f - t);

            // Voice ↑ quand on est proche
            SetVolume(_voiceHandle, t);
        }

        private void SetVolume(IAudioHandle handle, float volume)
        {
            if (handle?.CurrentSource == null) return;

            handle.CurrentSource.volume = volume;
        }

        #endregion
    }
}