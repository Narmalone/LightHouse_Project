using LightHouse.Core.Player;
using LightHouse.Core.Services;
using LightHouse.Features.Weather;
using UnityEngine;
using UnityEngine.Audio;

namespace LightHouse.Core.Audio
{
    /// <summary>
    /// Zone de vent autonome (mix piloté par la distance + météo).
    /// Deux modes de distance :
    /// - Virtual2D (par défaut) : épingle les sources au Listener (aucune atténuation 3D).
    /// - Real3D : garde la 3D et synchronise min/max avec inner/outer.
    /// 3 bandes crossfadées (Light/Medium/Heavy) + hystérésis + lissage + auto-stop.
    /// </summary>
    public class WindAmbienceZone : MonoBehaviour
    {
        public enum Band { Light, Medium, Heavy }
        public enum DistanceMode { Virtual2D_PinToListener, Real3D_SyncWithRadii }

        [Header("Clips par bande")]
        [SerializeField] private AudioCue lightWind;
        [SerializeField] private AudioCue mediumWind;
        [SerializeField] private AudioCue heavyWind;

        [Header("Listener & Zone")]
        public Transform Listener; // si null => main camera
        [Tooltip("Rayon plein niveau (poids=1)")]
        public float innerRadius = 20f;
        [Tooltip("Au-delà de ce rayon, la zone tombe à 0")]
        public float outerRadius = 80f;
        [Tooltip("Courbe de chute du poids de zone (t=0 outer → t=1 inner)")]
        public AnimationCurve zoneFalloff = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Distance Mode")]
        public DistanceMode distanceMode = DistanceMode.Virtual2D_PinToListener;
        [Tooltip("Si Real3D: coefficient (0..1) pour garder un peu de poids logiciel en plus de la 3D")]
        [Range(0f, 1f)] public float extraZoneWeightInReal3D = 0.25f;

        [Header("Entrée météo")]
        [Tooltip("WindSpeed (m/s si true, sinon km/h)")]
        public bool weatherSpeedIsMps = true;

        [Header("Seuils (km/h) + hystérésis")]
        [Tooltip("Seuil haut Light → Medium")]
        public float lightToMedium_Up = 20f;
        [Tooltip("Seuil haut Medium → Heavy")]
        public float mediumToHeavy_Up = 45f;
        [Tooltip("Marge quand on redescend (évite le flap)")]
        public float hysteresisDown = 4f;

        [Header("Largeur de crossfade (km/h)")]
        public float blendWidthLM = 10f;
        public float blendWidthMH = 12f;

        [Header("Gains artistiques")]
        [Range(0f, 2f)] public float lightGain = 0.9f;
        [Range(0f, 2f)] public float mediumGain = 1.0f;
        [Range(0f, 2f)] public float heavyGain = 1.1f;

        [Header("Smoothing & maintenance")]
        [Tooltip("Lissage exponentiel (par seconde). 0 = instantané")]
        public float smoothing = 6f;
        [Tooltip("Stoppe la couche si ~0 volume depuis X secondes")]
        public float autoStopAfterSilentSec = 2f;

        [Header("Optionnel: micro-gusts très doux")]
        public bool enableSubtleGusts = false;
        [Range(0f, 0.3f)] public float gustAmplitude = 0.08f;
        public float gustSpeed = 0.2f;
        public float gustPhase = 0f;

        [Header("Occlusion (roof check)")]
        public AudioMixer mixer;
        [Tooltip("Cutoff LPF quand on est à l'intérieur (fixe).")]
        public float indoorLPFHz = 1800f;
        [Tooltip("Cutoff LPF quand on est dehors (pas de coupe = 22 kHz).")]
        public float outdoorLPFHz = 22000f;

        // Handles runtime
        private IAudioHandle _hL, _hM, _hH;
        private float _tL, _tM, _tH;  // cibles (0..1 * gains * zoneWeight)
        private float _vL, _vM, _vH;  // volumes lissés
        private float _silL, _silM, _silH;

        // Cache
        private Transform _listenerCached;

        void Start()
        {
            if (!Listener)
            {
                var cam = Camera.main;
                Listener = cam ? cam.transform : transform;
            }
            _listenerCached = Listener;
            ValidateRadii();
        }

        void OnDestroy() => StopAll();

        void Update()
        {
            if (ServiceLocator.Audio == null || WeatherHandlerData.CurrentWeather == null) return;

            // Mixer LPF & volume indoor/outdoor
            bool occluded = PlayerHandlerData.IsPlayerOccluded();
            float lpf = Mathf.Clamp(occluded ? indoorLPFHz : outdoorLPFHz, 20f, 22000f);
            mixer.SetFloat("Wind_LPF_Cutoff", lpf);
            mixer.SetFloat("Wind_Volume", occluded ? -5f : 0f);

            // 1) Poids de zone (0..1)
            float zoneW = ComputeZoneWeight();

            // 2) Vent en km/h (pour seuils)
            float windInput = WeatherHandlerData.CurrentWeather.WindSpeed;
            float kmh = weatherSpeedIsMps ? windInput * 3.6f : windInput;

            // 3) Crossfade L/M/H (equal-power + hystérésis)
            ComputeBandTargets(kmh, zoneW);

            // 4) Assurer les handles si besoin
            EnsureHandles();

            // 5) Mode de distance (Virtual2D ou Real3D)
            ApplyDistanceMode(zoneW);

            // 6) Lissage + application
            TickVolumesAndApply();
        }

        float ComputeZoneWeight()
        {
            var lis = Listener ? Listener : _listenerCached;
            if (!lis) return 0f;

            float d = Vector3.Distance(lis.position, transform.position);
            if (d <= innerRadius) return 1f;
            if (d >= outerRadius) return 0f;

            // t: 0 à outer → 1 à inner (même sens que la courbe)
            float t = Mathf.InverseLerp(outerRadius, innerRadius, d);
            return Mathf.Clamp01(zoneFalloff.Evaluate(t));
        }

        void ComputeBandTargets(float kmh, float zoneW)
        {
            // Seuils "down" pour l'hystérésis
            float LM_down = Mathf.Max(0f, lightToMedium_Up - hysteresisDown);
            float MH_down = Mathf.Max(0f, mediumToHeavy_Up - hysteresisDown);

            float tgtL = 0f, tgtM = 0f, tgtH = 0f;

            if (kmh <= LM_down)
            {
                tgtL = 1f;
            }
            else if (kmh < lightToMedium_Up + blendWidthLM)
            {
                float u = Mathf.InverseLerp(LM_down, lightToMedium_Up + blendWidthLM, kmh);
                float s = Mathf.Sin(u * Mathf.PI * 0.5f);
                float c = Mathf.Cos(u * Mathf.PI * 0.5f);
                tgtL = c; tgtM = s;
            }
            else if (kmh <= MH_down)
            {
                tgtM = 1f;
            }
            else if (kmh < mediumToHeavy_Up + blendWidthMH)
            {
                float u = Mathf.InverseLerp(MH_down, mediumToHeavy_Up + blendWidthMH, kmh);
                float s = Mathf.Sin(u * Mathf.PI * 0.5f);
                float c = Mathf.Cos(u * Mathf.PI * 0.5f);
                tgtM = c; tgtH = s;
            }
            else
            {
                tgtH = 1f;
            }

            // Gusts subtils
            if (enableSubtleGusts && gustAmplitude > 0f)
            {
                float sG = Mathf.Sin((Time.time + gustPhase) * Mathf.PI * 2f * gustSpeed);
                float f = 1f + sG * gustAmplitude; // [1-A .. 1+A]
                tgtM *= f; tgtH *= f;
            }

            // Gains artistiques + poids de zone (le poids peut être ajusté en Real3D plus bas)
            _tL = tgtL * lightGain * zoneW;
            _tM = tgtM * mediumGain * zoneW;
            _tH = tgtH * heavyGain * zoneW;
        }

        void EnsureHandles()
        {
            if (_tL > 0.001f && _hL == null && lightWind != null)
            {
                _hL = ServiceLocator.Audio.PlayAt(lightWind, transform.position);
                _hL?.SetVolume(0f);
            }
            if (_tM > 0.001f && _hM == null && mediumWind != null)
            {
                _hM = ServiceLocator.Audio.PlayAt(mediumWind, transform.position);
                _hM?.SetVolume(0f);
            }
            if (_tH > 0.001f && _hH == null && heavyWind != null)
            {
                _hH = ServiceLocator.Audio.PlayAt(heavyWind, transform.position);
                _hH?.SetVolume(0f);
            }
        }

        void ApplyDistanceMode(float zoneW)
        {
            if (distanceMode == DistanceMode.Virtual2D_PinToListener)
            {
                // Neutralise toute atténuation 3D : on épingle au listener
                if (Listener)
                {
                    // TODO: si ton IAudioHandle ne propose pas SetPosition, remplace par ton API.
                    _hL?.SetPosition(Listener.position);
                    _hM?.SetPosition(Listener.position);
                    _hH?.SetPosition(Listener.position);
                }

                // TODO (optionnel): si ton handle permet SetSpatialBlend(0), fais-le ici
                // _hL?.SetSpatialBlend(0f); _hM?.SetSpatialBlend(0f); _hH?.SetSpatialBlend(0f);
            }
            else // Real3D_SyncWithRadii
            {
                // Synchronise min/max 3D aux rayons, garde un léger poids logiciel pour sculpter la pente.
                float extra = Mathf.Lerp(1f, extraZoneWeightInReal3D, 1f); // juste lisible

                // TODO: adapte à ton API si tu peux régler la 3D sur le handle/source
                // _hL?.SetRolloffLinear(); _hM?.SetRolloffLinear(); _hH?.SetRolloffLinear();
                // _hL?.Set3DMinMax(innerRadius, outerRadius);
                // _hM?.Set3DMinMax(innerRadius, outerRadius);
                // _hH?.Set3DMinMax(innerRadius, outerRadius);

                if (_hL != null)
                {
                    _hL.CurrentSource.minDistance = innerRadius;
                    _hL.CurrentSource.maxDistance = outerRadius;
                }


                // Option : garde un petit sculpt logiciel (sinon mets extraZoneWeightInReal3D=0)
                _tL *= extra; _tM *= extra; _tH *= extra;
            }
        }

        void TickVolumesAndApply()
        {
            float k = (smoothing > 0f) ? 1f - Mathf.Exp(-smoothing * Time.deltaTime) : 1f;

            _vL = Mathf.Lerp(_vL, _tL, k);
            _vM = Mathf.Lerp(_vM, _tM, k);
            _vH = Mathf.Lerp(_vH, _tH, k);

            _hL?.SetVolume(_vL);
            _hM?.SetVolume(_vM);
            _hH?.SetVolume(_vH);

            HandleAutoStop(ref _hL, _vL, ref _silL);
            HandleAutoStop(ref _hM, _vM, ref _silM);
            HandleAutoStop(ref _hH, _vH, ref _silH);
        }

        void HandleAutoStop(ref IAudioHandle h, float vol, ref float timer)
        {
            if (h == null) { timer = 0f; return; }
            if (vol <= 0.002f)
            {
                timer += Time.deltaTime;
                if (timer >= autoStopAfterSilentSec) { h.Stop(); h = null; timer = 0f; }
            }
            else timer = 0f;
        }

        void StopAll()
        {
            _hL?.Stop(); _hL = null; _vL = _tL = 0f; _silL = 0f;
            _hM?.Stop(); _hM = null; _vM = _tM = 0f; _silM = 0f;
            _hH?.Stop(); _hH = null; _vH = _tH = 0f; _silH = 0f;
        }

        void OnValidate()
        {
            ValidateRadii();

            lightToMedium_Up = Mathf.Max(0f, lightToMedium_Up);
            mediumToHeavy_Up = Mathf.Max(lightToMedium_Up + 0.01f, mediumToHeavy_Up);
            hysteresisDown = Mathf.Clamp(hysteresisDown, 0f, 20f);

            blendWidthLM = Mathf.Max(0f, blendWidthLM);
            blendWidthMH = Mathf.Max(0f, blendWidthMH);

            indoorLPFHz = Mathf.Clamp(indoorLPFHz, 20f, 22000f);
            outdoorLPFHz = Mathf.Clamp(outdoorLPFHz, 20f, 22000f);

            // courbe minimale : évite qu'elle soit plate
            if (zoneFalloff == null || zoneFalloff.length < 2)
                zoneFalloff = AnimationCurve.EaseInOut(0, 0, 1, 1);
        }

        private void ValidateRadii()
        {
            if (outerRadius < innerRadius) outerRadius = innerRadius;
            innerRadius = Mathf.Max(0.01f, innerRadius);
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.3f, 0.7f, 1f, 0.25f);
            Gizmos.DrawWireSphere(transform.position, innerRadius);
            Gizmos.color = new Color(0.3f, 0.7f, 1f, 0.10f);
            Gizmos.DrawWireSphere(transform.position, outerRadius);
        }
#endif
    }

}
