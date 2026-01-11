using LightHouse.Core.Audio;
using LightHouse.Core.Services;
using LightHouse.Features.Nightwatch;
using LightHouse.Features.TimeOfDay.TimeCore;
using System.Collections;
using UnityEngine;

namespace LightHouse.Features.LightHouse
{
    public class LightHouseController : MonoBehaviour
    {
        [SerializeField] private LightHouseLight _mainLight;

        [Header("Horn Settings")]
        [SerializeField] private AudioCue _hornCue;

        [SerializeField] private SO_NightWatchConfiguration _nightwatchConfig;

        [Tooltip("Nombre de répétitions du son à chaque séquence")]
        [SerializeField] private int _numberOfRepeats = 6;

        [SerializeField] private bool _hornPlayedToday = false;

        private Coroutine _hornRoutine;
        private IAudioHandle _audioHandle;

        private void Awake()
        {
            TimeHandlerData.OnDayChanged += OndayChanged;
        }

        private void OndayChanged(byte obj)
        {
            _hornPlayedToday = false;
        }

        private void LateUpdate()
        {
            if (_hornPlayedToday) return;
            if (TimeHandlerData.CurrentTime >= _nightwatchConfig.StartHour && _hornRoutine == null)
            {
                _hornRoutine = StartCoroutine(HornSequence());
                _mainLight.StartLight();
            }
        }

        private void OnDestroy()
        {
            TimeHandlerData.OnDayChanged -= OndayChanged;
        }

        private IEnumerator HornSequence()
        {
            yield return new WaitForSeconds(Random.Range(1f, 3.5f));
            for (int i = 0; i < _numberOfRepeats; i++)
            {
                // Lance un coup
                _audioHandle = StartHorn();

                // Durée basée sur le clip (corrigée par le pitch si dispo)
                float dur = EstimateClipDurationSeconds(_audioHandle, _hornCue);
                if (dur <= 0f) dur = 0.1f; // filet de sécurité
                                           // On utilise Realtime pour ignorer le timescale éventuel
                yield return new WaitForSecondsRealtime(dur + 0.05f); // petite marge pour éviter les cut-offs
            }

            _hornPlayedToday = true;
            _hornRoutine = null;
        }

        private IAudioHandle StartHorn()
        {
            if (ServiceLocator.Audio != null && _hornCue != null)
            {
                var handle = ServiceLocator.Audio.PlayAt(_hornCue, transform.position);
                // Optionnel: s'assurer qu'il ne loop pas (si ton handle expose ce réglage)
                // handle.Loop = false;
                return handle;
            }
            else
            {
                Debug.LogWarning("[LightHouse] HornCue or Audio Service missing!");
                return null;
            }
        }

        /// <summary>
        /// Essaie de récupérer la durée réelle du clip joué. Si ton IAudioHandle ne fournit pas SelectedClip/Pitch,
        /// remplace par ce que ton AudioCue expose (ex: HornCue.Clip.length).
        /// </summary>
        private static float EstimateClipDurationSeconds(IAudioHandle handle, AudioCue cueFallback)
        {
            float pitch = 1f;
            AudioClip clip = null;

            // Adapte ces accès à ton API réelle :
            if (handle != null)
            {
                clip = handle.SelectedClip;         // ← remplace par la propriété réelle si besoin
                pitch = handle.CurrentSource.pitch != 0f ? handle.CurrentSource.pitch : 1f; // ← remplace par la propriété réelle si besoin
            }

            if (clip != null)
                return Mathf.Max(0f, clip.length / Mathf.Max(0.001f, pitch));

            return 0f; // inconnu
        }
    }
}

