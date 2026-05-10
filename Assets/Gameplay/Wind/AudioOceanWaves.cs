using LightHouse.Core.Player;
using LightHouse.Core.Services;
using LightHouse.Features.Weather;
using UnityEngine;
using UnityEngine.Audio;

namespace LightHouse.Core.Audio
{
    /// <summary>
    /// Emetteur de vagues "autonome":
    /// - Lit la vitesse du vent de la météo.
    /// - Détermine un mix Light/Medium/Heavy avec hystérésis + crossfade equal-power.
    /// - Joue/stoppe les loops nécessaires via PlayAt(transform.position).
    /// - Lisse les volumes et auto-stoppe les couches muettes (perf).
    /// Place plusieurs instances de ce script le long du rivage.
    /// </summary>
    public class AudioOceanWaves : MonoBehaviour
    {
        public enum WaveBand { Light, Medium, Heavy }

        [Header("Clips par bande")]
        [SerializeField] private SO_AudioCue _lightWaves;
        [SerializeField] private SO_AudioCue _mediumWaves;
        [SerializeField] private SO_AudioCue _heavyWaves;

        [Header("Entrée météo")]
        [Tooltip("Si vrai, WindSpeed est en m/s; sinon elle est déjà en km/h.")]
        public bool weatherSpeedIsMps = true;

        [Header("Seuils (km/h) + hystérésis")]
        [Tooltip("Au-dessus → on commence à quitter Light vers Medium.")]
        public float lightToMedium_Up = 35f;
        [Tooltip("Au-dessus → on commence à quitter Medium vers Heavy.")]
        public float mediumToHeavy_Up = 65f;
        [Tooltip("Marge en km/h quand on redescend (évite le flap).")]
        public float hysteresisDown = 5f;

        [Header("Largeur de crossfade (km/h)")]
        [Tooltip("Largeur du fondu Light↔Medium autour du seuil")]
        public float blendWidth_LM = 15f;
        [Tooltip("Largeur du fondu Medium↔Heavy autour du seuil")]
        public float blendWidth_MH = 20f;

        [Header("Gains artistiques (par bande)")]
        [Range(0f, 2f)] public float lightGain = 0.9f;
        [Range(0f, 2f)] public float mediumGain = 1.0f;
        [Range(0f, 2f)] public float heavyGain = 1.15f;

        [Header("Lissage & housekeeping")]
        [Tooltip("Lissage exponentiel des volumes (par seconde). 0 = instantané")]
        public float smoothing = 6f;
        [Tooltip("Stoppe la couche si son volume reste ~0 pendant X secondes.")]
        public float autoStopAfterSilentSec = 1.5f;

        // ---- Runtime ----
        private IAudioHandle _hLight, _hMed, _hHeavy;
        private float _tLight, _tMed, _tHeavy;      // cibles (0..1 * gain)
        private float _vLight, _vMed, _vHeavy;      // volumes lissés
        private float _silentLight, _silentMed, _silentHeavy;

        [Header("Occlusion (roof check)")]
        [Tooltip("Cutoff LPF quand on est à l'intérieur (fixe).")]
        public float indoorLPFHz = 1800f;
        [Tooltip("Cutoff LPF quand on est dehors (pas de coupe = 22 kHz).")]
        public float outdoorLPFHz = 22000f;

        public AudioMixer mixer;

        void OnDisable()
        {
            StopAll();
        }

        void Update()
        {
            if (ServiceLocator.Audio == null || WeatherHandlerData.CurrentWeather == null)
                return;

            // 1) Lire la vitesse vent (km/h)
            float input = WeatherHandlerData.CurrentWeather.WindSpeed;
            float kmh = weatherSpeedIsMps ? input * 3.6f : input;

            // 2) Déterminer les cibles de mix par bande (equal-power + hystérésis)
            ComputeTargets(kmh);

            // 3) Assurer les handles actifs pour les couches utiles
            EnsureHandles();

            // 4) Lisser et appliquer volumes
            TickVolumesAndApply();

            float lpf = PlayerHandlerData.IsPlayerOccluded() ? Mathf.Clamp(indoorLPFHz, 20f, 22000f)
                                 : Mathf.Clamp(outdoorLPFHz, 20f, 22000f);
            mixer.SetFloat("Waves_LPF_Cutoff", lpf);

        }

        // ------------------------------ Mix logic ------------------------------
        private void ComputeTargets(float kmh)
        {
            // Seuils down (hystérésis)
            float LM_down = Mathf.Max(0f, lightToMedium_Up - hysteresisDown);
            float MH_down = Mathf.Max(0f, mediumToHeavy_Up - hysteresisDown);

            // Reset cibles
            float tgtL = 0f, tgtM = 0f, tgtH = 0f;

            // Zones piecewise avec fondu equal-power (cos/sin)
            if (kmh <= LM_down)
            {
                // Light pur
                tgtL = 1f; tgtM = 0f; tgtH = 0f;
            }
            else if (kmh < lightToMedium_Up + blendWidth_LM)
            {
                // Fondu Light -> Medium
                float u = Mathf.InverseLerp(LM_down, lightToMedium_Up + blendWidth_LM, kmh); // 0..1
                float a = Mathf.Cos(u * Mathf.PI * 0.5f); // Light
                float b = Mathf.Sin(u * Mathf.PI * 0.5f); // Medium
                tgtL = a; tgtM = b; tgtH = 0f;
            }
            else if (kmh <= MH_down)
            {
                // Medium pur
                tgtL = 0f; tgtM = 1f; tgtH = 0f;
            }
            else if (kmh < mediumToHeavy_Up + blendWidth_MH)
            {
                // Fondu Medium -> Heavy
                float u = Mathf.InverseLerp(MH_down, mediumToHeavy_Up + blendWidth_MH, kmh); // 0..1
                float a = Mathf.Cos(u * Mathf.PI * 0.5f); // Medium
                float b = Mathf.Sin(u * Mathf.PI * 0.5f); // Heavy
                tgtL = 0f; tgtM = a; tgtH = b;
            }
            else
            {
                // Heavy pur
                tgtL = 0f; tgtM = 0f; tgtH = 1f;
            }

            // Gains artistiques
            _tLight = tgtL * lightGain;
            _tMed = tgtM * mediumGain;
            _tHeavy = tgtH * heavyGain;
        }

        // ------------------------------ Handles ------------------------------
        private void EnsureHandles()
        {
            // Light
            if (_tLight > 0.001f && _hLight == null && _lightWaves != null)
            {
                _hLight = ServiceLocator.Audio.PlayAt(_lightWaves, transform.position);
                _hLight?.SetVolume(0f);
            }
            // Medium
            if (_tMed > 0.001f && _hMed == null && _mediumWaves != null)
            {
                _hMed = ServiceLocator.Audio.PlayAt(_mediumWaves, transform.position);
                _hMed?.SetVolume(0f);
            }
            // Heavy
            if (_tHeavy > 0.001f && _hHeavy == null && _heavyWaves != null)
            {
                _hHeavy = ServiceLocator.Audio.PlayAt(_heavyWaves, transform.position);
                _hHeavy?.SetVolume(0f);
            }
        }

        // ------------------------- Lissage + apply ---------------------------
        private void TickVolumesAndApply()
        {
            float k = (smoothing > 0f) ? (1f - Mathf.Exp(-smoothing * Time.deltaTime)) : 1f;

            _vLight = Mathf.Lerp(_vLight, _tLight, k);
            _vMed = Mathf.Lerp(_vMed, _tMed, k);
            _vHeavy = Mathf.Lerp(_vHeavy, _tHeavy, k);

            // Apply
            _hLight?.SetVolume(_vLight);
            _hMed?.SetVolume(_vMed);
            _hHeavy?.SetVolume(_vHeavy);

            // Auto-stop silencieux
            HandleAutoStop(ref _hLight, _vLight, ref _silentLight);
            HandleAutoStop(ref _hMed, _vMed, ref _silentMed);
            HandleAutoStop(ref _hHeavy, _vHeavy, ref _silentHeavy);
        }

        private void HandleAutoStop(ref IAudioHandle h, float vol, ref float silentTimer)
        {
            if (h == null) { silentTimer = 0f; return; }

            if (vol <= 0.002f)
            {
                silentTimer += Time.deltaTime;
                if (silentTimer >= autoStopAfterSilentSec)
                {
                    h.Stop();
                    h = null;
                    silentTimer = 0f;
                }
            }
            else
            {
                silentTimer = 0f;
            }
        }

        private void StopAll()
        {
            _hLight?.Stop(); _hLight = null; _vLight = _tLight = 0f; _silentLight = 0f;
            _hMed?.Stop(); _hMed = null; _vMed = _tMed = 0f; _silentMed = 0f;
            _hHeavy?.Stop(); _hHeavy = null; _vHeavy = _tHeavy = 0f; _silentHeavy = 0f;
        }
    }
}

