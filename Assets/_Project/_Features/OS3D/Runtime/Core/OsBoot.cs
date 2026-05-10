using LightHouse.Core.Audio;
using LightHouse.Core.Services;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Features.Computer.OS
{
    public class OsBoot : MonoBehaviour
    {
        [Header("UI")]
        public Slider slider;
        public CanvasGroup osBootGroup;

        [Header("Timing")]
        [Tooltip("1 = durée identique au clip. >1 = plus rapide, <1 = plus lent.")]
        public float Speed = 1.0f;

        [Header("Audio")]
        [SerializeField] private SO_AudioCue _startBoot;

        private float _timer = 0.0f;
        private IAudioHandle _startUpAudioHandle;

        private void Start()
        {
            // Slider borné et initialisé
            if (slider != null)
            {
                slider.minValue = 0f;
                slider.maxValue = 1f;
                slider.value = 0f;
            }
            Hide();
        }

        public void StartBoot(Action onBootCompleted)
        {
            Show();

            if (ServiceLocator.Audio != null)
            {
                _startUpAudioHandle = ServiceLocator.Audio.PlayAt(_startBoot, this.transform.position);
            }

            // Reset timer pour un nouveau boot
            _timer = 0f;
            if (slider) slider.value = 0f;

            StartCoroutine(BootRoutine(onBootCompleted));
        }

        public void Hide()
        {
            osBootGroup.alpha = 0.0f;
            osBootGroup.interactable = false;
            osBootGroup.blocksRaycasts = false;
        }

        public void Show()
        {
            osBootGroup.alpha = 1.0f;
            osBootGroup.interactable = true;
            osBootGroup.blocksRaycasts = true;
        }

        public IEnumerator BootRoutine(Action onBootCompleted)
        {
            // Récupère la durée du clip (fallback 3s si rien)
            float clipLength = 3f;
            if (_startUpAudioHandle != null && _startUpAudioHandle.SelectedClip != null)
                clipLength = _startUpAudioHandle.SelectedClip.length - 0.2f;

            // Applique le facteur Speed (Speed>1 = plus vite, donc durée plus courte)
            float duration = Mathf.Max(0.01f, clipLength / Mathf.Max(0.001f, Speed));

            _timer = 0f;

            while (_timer < duration)
            {
                _timer += Time.deltaTime;

                // Normalisation 0..1 du temps
                float t01 = Mathf.Clamp01(_timer / duration);

                if (slider) slider.value = t01;

                yield return null;
            }

            if (slider) slider.value = 1f; // assure le 1.0 en fin

            Hide();
            onBootCompleted?.Invoke();
        }
    }
}

