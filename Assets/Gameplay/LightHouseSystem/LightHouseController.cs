using LightHouse.Core.Audio;
using LightHouse.Core.Services;
using LightHouse.Features.Nightwatch;
using LightHouse.Features.TimeOfDay.TimeCore;
using System.Collections;
using UnityEngine;

namespace LightHouse.Features.LightHouse
{
    public sealed class LightHouseController : MonoBehaviour
    {
        [Header("Lights")]
        [SerializeField] private LightHouseLight _mainLight;
        [SerializeField] private Light _pointLight;

        [Header("Horn")]
        [SerializeField] private AudioCue _hornCue;
        [SerializeField, Min(1)] private int _numberOfRepeats = 6;

        [Header("Config")]
        [SerializeField] private SO_NightWatchConfiguration _nightwatchConfig;

        private bool _hornPlayedToday;
        private bool _isNightwatchActive;
        private Coroutine _hornRoutine;

        private void Awake()
        {
            TimeHandlerData.OnDayChanged += OnDayChanged;
            _hornPlayedToday = false;
            // Optionnel : synchronise l'état dès l'activation
            RefreshState(force: true);
        }

        private void OnDestroy()
        {
            TimeHandlerData.OnDayChanged -= OnDayChanged;
            StopHornSequence();
            SetLights(false);
        }

        private void OnDayChanged(byte _)
        {
            _hornPlayedToday = false;
        }

        private void Update()
        {
            RefreshState(force: false);
        }

        private void RefreshState(bool force)
        {
            bool shouldBeActive = IsWithinNightwatchWindow(TimeHandlerData.CurrentTime);
            if (force || shouldBeActive != _isNightwatchActive)
            {
                _isNightwatchActive = shouldBeActive;

                if (_isNightwatchActive)
                {
                    // Entrée dans la fenêtre Nightwatch
                    SetLights(true);

                    if (!_hornPlayedToday && _hornRoutine == null)
                        _hornRoutine = StartCoroutine(HornSequence());
                }
                else
                {
                    // Sortie de la fenêtre Nightwatch
                    StopHornSequence();
                    SetLights(false);
                }
            }
        }

        private bool IsWithinNightwatchWindow(float currentHour)
        {
            // Cas simple (ex: 18 -> 23)
            // Cas qui traverse minuit (ex: 20 -> 6)
            float start = _nightwatchConfig.StartHour;
            float end = _nightwatchConfig.EndHour;

            if (start <= end)
                return currentHour >= start && currentHour <= end;

            // traverse minuit
            return currentHour >= start || currentHour <= end;
        }

        private void SetLights(bool enabled)
        {
            if (_mainLight != null)
            {
                if (enabled) _mainLight.StartLight();
                else _mainLight.StopLight();
            }

            if (_pointLight != null)
                _pointLight.enabled = enabled;
        }

        private void StopHornSequence()
        {
            if (_hornRoutine != null)
            {
                StopCoroutine(_hornRoutine);
                _hornRoutine = null;
            }
        }

        private IEnumerator HornSequence()
        {
            // Petit délai aléatoire
            yield return new WaitForSeconds(Random.Range(1f, 3.5f));

            for (int i = 0; i < _numberOfRepeats; i++)
            {
                // Si on est sorti de la fenêtre pendant la séquence, on stop
                if (!_isNightwatchActive)
                    break;

                var handle = StartHorn();
                float dur = EstimateClipDurationSeconds(handle, _hornCue);
                if (dur <= 0f) dur = 0.25f;

                yield return new WaitForSecondsRealtime(dur + 0.05f);
            }

            if (_isNightwatchActive)
                _hornPlayedToday = true;

            _hornRoutine = null;
        }

        private IAudioHandle StartHorn()
        {
            if (ServiceLocator.Audio == null || _hornCue == null)
            {
                Debug.LogWarning("[LightHouse] HornCue or Audio Service missing!");
                return null;
            }

            return ServiceLocator.Audio.PlayAt(_hornCue, transform.position);
        }

        /// <summary>
        /// Récupère la durée du clip joué si possible.
        /// Adapte cette méthode à ton API audio (AudioCue expose peut-être un clip / une durée).
        /// </summary>
        private static float EstimateClipDurationSeconds(IAudioHandle handle, AudioCue cueFallback)
        {
            // --- À ADAPTER selon ton système audio ---
            // Si ton handle expose un clip / une durée / un pitch, utilise-les ici.
            // Exemple (fictif) :
            var clip = handle?.SelectedClip;

            // Fallback simple si tu peux obtenir un clip via le cue (selon ton implémentation) :
            // return clip != null ? clip.length : 0f;
            if (clip != null)
                return clip.length;

            return 0f;
        }
    }
}
