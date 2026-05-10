using LightHouse.Core.Player;
using LightHouse.Core.Services;
using LightHouse.Features.Weather;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace LightHouse.Core.Audio
{
    public class WavesAudioController : MonoBehaviour
    {
        [Header("State (0..1)")]
        [Range(0, 1)] public float SeaState = 0f;     // force de la mer / houle (pilotée auto ci-dessous)
        public float WindSpeed = 0f;                  // m/s (mis à jour depuis météo pour le "hiss")
        [Range(0, 1)] public float Exposure = 1f;     // 1 = côte ouverte au large

        [Header("Player / Distance")]
        public Transform Listener;
        public float nearRange = 30f;
        public float farRange = 300f;
        public List<Collider> ShoreZones_Rock = new();
        public List<Collider> ShoreZones_Sand = new();

        [Header("Crossover SeaState → Layers")]
        [Range(0f, 1f)] public float seaLM = 0.35f;
        [Range(0f, 1f)] public float seaMH = 0.70f;
        [Range(0f, 1f)] public float equalizeStart = 0.90f;

        [Header("Layer trims (pour équilibrer tes assets)")]
        [Range(0f, 2f)] public float lightTrim = 1f;
        [Range(0f, 2f)] public float medTrim = 1f;
        [Range(0f, 2f)] public float heavyTrim = 1f;

        [Header("Master (distance)")]
        public AnimationCurve masterFromDistance = new AnimationCurve(
            new Keyframe(0f, 1.00f),
            new Keyframe(50f, 0.80f),
            new Keyframe(150f, 0.55f),
            new Keyframe(300f, 0.35f)
        );

        [Header("Audio Mixer")]
        public AudioMixer mixer; // expose: Waves_Wash_Light, Waves_Wash_Med, Waves_Wash_Heavy, Waves_Distant

        [Header("Loops")]
        public SO_AudioCue _loopWashLightCue;
        public SO_AudioCue _loopWashMedCue;
        public SO_AudioCue _loopWashHeavyCue;
        public SO_AudioCue _loopWashDistantCue;
        public IAudioHandle loopWashLight;
        public IAudioHandle loopWashMed;
        public IAudioHandle loopWashHeavy;
        public IAudioHandle loopDistant;

        [Header("One-shots Crash")]
        public AudioSource crashPrefab;
        public SO_AudioCue rockCrashes;
        public SO_AudioCue sandCrashes;
        public Vector2 crashIntervalRange = new(2.5f, 8f); // à SeaState=1
        public AnimationCurve crashRateFromSea = AnimationCurve.Linear(0, 0, 1, 1);
        public AnimationCurve crashGainByDistance = new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(50f, 0.8f),
            new Keyframe(150f, 0.5f),
            new Keyframe(300f, 0.25f)
        );

        [Header("Volumes (curves)")]
        // Multiplicateurs après le crossfade par SeaState.
        public AnimationCurve washLightFromSea = AnimationCurve.Linear(0, 0.25f, 1, 0.6f);
        public AnimationCurve washMedFromSea = AnimationCurve.Linear(0, 0.0f, 1, 0.9f);
        public AnimationCurve washHeavyFromSea = AnimationCurve.Linear(0, 0.0f, 1, 1.0f);
        public AnimationCurve hissFromWind = AnimationCurve.Linear(0, 0.0f, 20, 1.0f);
        public AnimationCurve distantFromSea = AnimationCurve.Linear(0, 0.1f, 1, 1.0f);

        [Header("Smoothing / LPF")]
        public float volSmooth = 0.25f;
        [Header("Occlusion (roof check)")]
        [Tooltip("Cutoff LPF quand on est à l'intérieur (fixe).")]
        public float indoorLPFHz = 1800f;
        [Tooltip("Cutoff LPF quand on est dehors (pas de coupe = 22 kHz).")]
        public float outdoorLPFHz = 22000f;

        // ---------- Couplage météo → SeaState ----------
        [Header("Weather coupling (vent → SeaState)")]
        public bool driveSeaFromWeather = true;
        [Tooltip("Coche si Weather.WindSpeed est en km/h. (Tu dis max ~120-130 → probablement km/h)")]
        public bool weatherWindIsKph = true;
        [Tooltip("Vitesse de vent de référence (km/h si 'weatherWindIsKph', sinon m/s) qui donne SeaState=1.")]
        public float maxWindRef = 130f;

        [Tooltip("Courbe vent → SeaState (0..maxWindRef → 0..1). Par défaut un profil façon Beaufort simplifié.")]
        public AnimationCurve seaFromWind = new AnimationCurve(
            new Keyframe(0f, 0.00f),
            new Keyframe(10f, 0.08f),
            new Keyframe(20f, 0.20f),
            new Keyframe(40f, 0.45f),
            new Keyframe(60f, 0.65f),
            new Keyframe(80f, 0.80f),
            new Keyframe(100f, 0.90f),
            new Keyframe(130f, 1.00f)
        );

        [Tooltip("Temps de montée (s) vers une mer plus forte).")]
        [Range(0f, 10f)] public float seaRiseTau = 1.5f;
        [Tooltip("Temps de descente (s) quand le vent tombe).")]
        [Range(0f, 10f)] public float seaFallTau = 3.0f;

        // privates
        float _vL, _vM, _vH, _vRum;
        float _dL, _dM, _dH, _dRum;
        Coroutine _crashLoop;
        float _seaVel; // pour SmoothDamp du SeaState

        private void Awake()
        {
            WeatherHandlerData.OnWeatherTypeChanged += OnWeatherTypeChanged;
        }

        void Start()
        {
            if (!Listener) Listener = Camera.main ? Camera.main.transform : transform;
            EnsurePlaying(_loopWashLightCue, _loopWashMedCue, _loopWashHeavyCue, _loopWashDistantCue);
            _crashLoop = StartCoroutine(CrashScheduler());
        }

        void Update()
        {
            // --- Met à jour SeaState & WindSpeed depuis la météo (si demandé)
            if (driveSeaFromWeather && WeatherHandlerData.CurrentWeather != null)
            {
                float w = WeatherHandlerData.CurrentWeather.WindSpeed; // unité: dépend de ton jeu
                                                                       // 1) Met à jour WindSpeed en m/s pour le "hiss"
                WindSpeed = weatherWindIsKph ? (w * 0.27778f) : w;

                // 2) Calcule la cible SeaState depuis la courbe
                float wForCurve = weatherWindIsKph ? w : w; // si ta courbe est en m/s, change maxWindRef et les clés.
                float clampedW = Mathf.Clamp(wForCurve, 0f, Mathf.Max(1f, maxWindRef));
                float targetSea = Mathf.Clamp01(seaFromWind.Evaluate(clampedW));

                // optionnel: boost léger par côtes exposées
                targetSea *= Mathf.Lerp(0.85f, 1.0f, Exposure);

                // 3) Lissage montée/descente (montée plus rapide que descente)
                float tau = (targetSea >= SeaState) ? seaRiseTau : seaFallTau;
                SeaState = (tau > 0f)
                    ? Mathf.SmoothDamp(SeaState, targetSea, ref _seaVel, tau)
                    : targetSea;
            }

            DriveLoops();
        }

        void OnDestroy()
        {
            WeatherHandlerData.OnWeatherTypeChanged -= OnWeatherTypeChanged;
            if (_crashLoop != null) StopCoroutine(_crashLoop);
        }

        private void OnWeatherTypeChanged(WeatherType type)
        {
            // Ici tu peux éventuellement “kick” la mer quand le type de météo change,
            // ex: réinitialiser la vélocité de lissage :
            _seaVel = 0f;
        }

        // -------- Core --------
        void DriveLoops()
        {
            float sea = Mathf.Clamp01(SeaState);

            float dist = DistanceToNearestShore(Listener.position, out bool isRock);
            float df = Mathf.Clamp01(Mathf.InverseLerp(nearRange, farRange, dist)); // 0 près, 1 loin

            // CROSSFADE SeaState → parts Light/Med/Heavy
            ComputeLayerWeights(sea, out float partL, out float partM, out float partH);

            // Gains “base” + trims
            float baseL = washLightFromSea.Evaluate(sea) * lightTrim;
            float baseM = washMedFromSea.Evaluate(sea) * medTrim;
            float baseH = washHeavyFromSea.Evaluate(sea) * heavyTrim;

            float wL = partL * baseL;
            float wM = partM * baseM;
            float wH = partH * baseH;

            // Côte exposée → un peu plus d’Heavy
            wH *= Mathf.Lerp(0.7f, 1f, Exposure);

            // Master distance
            float master = Mathf.Clamp01(masterFromDistance.Evaluate(dist));
            wL *= master * Mathf.Lerp(0.6f, 1f, 1f - df);
            wM *= master * Mathf.Lerp(0.5f, 1f, 1f - df);
            wH *= master * Mathf.Lerp(0.4f, 1f, 1f - df);

            // Hiss vent (proche)
            float hiss = hissFromWind.Evaluate(WindSpeed);
            wL = Mathf.Clamp01(wL + 0.20f * hiss * (1f - df));
            wM = Mathf.Clamp01(wM + 0.10f * hiss * (1f - df));

            // Plage adoucit Med/Heavy
            if (!isRock) { wM *= 0.85f; wH *= 0.65f; }

            // Distant rumble (loin + mer forte + côte exposée)
            float rumble = distantFromSea.Evaluate(sea)
                         * Mathf.Lerp(0.2f, 1f, df)
                         * Mathf.Lerp(0.5f, 1f, Exposure);

            // smoothing
            _vL = (volSmooth > 0f) ? Mathf.SmoothDamp(_vL, wL, ref _dL, volSmooth) : wL;
            _vM = (volSmooth > 0f) ? Mathf.SmoothDamp(_vM, wM, ref _dM, volSmooth) : wM;
            _vH = (volSmooth > 0f) ? Mathf.SmoothDamp(_vH, wH, ref _dH, volSmooth) : wH;
            _vRum = (volSmooth > 0f) ? Mathf.SmoothDamp(_vRum, rumble, ref _dRum, volSmooth) : rumble;

            // LPF distance + occlusion simple
            float lpf = PlayerHandlerData.IsPlayerOccluded() ? Mathf.Clamp(indoorLPFHz, 20f, 22000f)
                                 : Mathf.Clamp(outdoorLPFHz, 20f, 22000f);

            // mixer
            if (mixer)
            {
                mixer.SetFloat("Waves_Wash_Light", Linear01ToDb(_vL));
                mixer.SetFloat("Waves_Wash_Med", Linear01ToDb(_vM));
                mixer.SetFloat("Waves_Wash_Heavy", Linear01ToDb(_vH));
                mixer.SetFloat("Waves_Distant", Linear01ToDb(_vRum));
                mixer.SetFloat("Waves_LPF_Cutoff", lpf);
            }

            // volumes locaux (si non routés via mixer)
            if (loopWashLight != null) loopWashLight.SetVolume(_vL);
            if (loopWashMed != null) loopWashMed.SetVolume(_vM);
            if (loopWashHeavy != null) loopWashHeavy.SetVolume(_vH);
            if (loopDistant != null) loopDistant.SetVolume(_vRum);
        }

        // --- Crossfade SeaState → parts L/M/H (somme ≈ 1)
        void ComputeLayerWeights(float sea, out float l, out float m, out float h)
        {
            sea = Mathf.Clamp01(sea);

            float lightFall = Mathf.Clamp01((seaLM - sea) / Mathf.Max(seaLM, 1e-6f));
            float medRise = Mathf.Clamp01((sea - seaLM) / Mathf.Max(seaMH - seaLM, 1e-6f));
            float heavyRise = Mathf.Clamp01((sea - seaMH) / Mathf.Max(1f - seaMH, 1e-6f));

            lightFall = Smooth01(lightFall);
            medRise = Smooth01(medRise);
            heavyRise = Smooth01(heavyRise);

            l = Mathf.Lerp(0.15f, 1f, lightFall);
            m = medRise;
            h = heavyRise;

            float s = l + m + h;
            if (s > 1e-6f) { l /= s; m /= s; h /= s; }

            if (sea >= equalizeStart)
            {
                float t = Mathf.Clamp01((sea - equalizeStart) / Mathf.Max(1f - equalizeStart, 1e-6f));
                float oneThird = 1f / 3f;
                l = Mathf.Lerp(l, oneThird, t);
                m = Mathf.Lerp(m, oneThird, t);
                h = Mathf.Lerp(h, oneThird, t);
            }
        }

        static float Smooth01(float t)
        {
            t = Mathf.Clamp01(t);
            return t * t * (3f - 2f * t);
        }

        IEnumerator CrashScheduler()
        {
            var wait = new WaitForEndOfFrame();
            while (true)
            {
                float sea = Mathf.Clamp01(SeaState);
                float rate = crashRateFromSea.Evaluate(sea) * Mathf.Lerp(0.3f, 1f, Exposure);
                if (rate <= 0.01f) { yield return new WaitForSeconds(1f); continue; }

                float min = Mathf.Lerp(6f, crashIntervalRange.x, sea);
                float max = Mathf.Lerp(12f, crashIntervalRange.y, sea);
                float interval = Random.Range(min, max) / Mathf.Max(0.2f, rate);

                float t = 0f;
                while (t < interval) { t += Time.deltaTime; yield return wait; }

                TrySpawnCrash();
            }
        }

        void TrySpawnCrash()
        {
            if (!crashPrefab) return;

            Vector3 listenerPos = Listener ? Listener.position : transform.position;
            bool rock;
            Vector3 pos = RandomShorePointNear(listenerPos, out rock);
            float dist = Vector3.Distance(listenerPos, pos);

            float sea = Mathf.Clamp01(SeaState);
            float distGain = crashGainByDistance.Evaluate(dist);
            float base01 = Mathf.Clamp01(0.5f * sea * distGain);

            var targetCue = rock ? rockCrashes : sandCrashes;
            if (ServiceLocator.Audio != null)
            {
                var s = ServiceLocator.Audio.PlayAt(targetCue, pos);
                s.SetPitch(Random.Range(0.95f, 1.05f));
                // s.SetVolume(base01); // si ton AudioCue le permet
            }
        }

        // -------- Distance & positions utilitaires --------
        float DistanceToNearestShore(Vector3 p, out bool isRock)
        {
            float best = float.MaxValue; isRock = true;
            Vector3 dummy;

            foreach (var c in ShoreZones_Rock)
                if (c) best = Mathf.Min(best, DistanceToColliderXZ(c, p, out dummy));

            float bestSand = float.MaxValue;
            foreach (var c in ShoreZones_Sand)
                if (c) bestSand = Mathf.Min(bestSand, DistanceToColliderXZ(c, p, out dummy));

            if (bestSand < best) { best = bestSand; isRock = false; }
            return best;
        }

        Vector3 NearestShorePoint(Vector3 p)
        {
            Vector3 bestP = p; float best = float.MaxValue; Vector3 q;

            foreach (var c in ShoreZones_Rock)
                if (c && DistanceToColliderXZ(c, p, out q) < best) { best = p.DistanceXZ(q); bestP = q; }
            foreach (var c in ShoreZones_Sand)
                if (c && DistanceToColliderXZ(c, p, out q) < best) { best = p.DistanceXZ(q); bestP = q; }

            return bestP;
        }

        Vector3 RandomShorePointNear(Vector3 p, out bool isRock)
        {
            isRock = true;
            Collider best = null; float bestD = float.MaxValue; Vector3 _;

            foreach (var c in ShoreZones_Rock)
                if (c) { float d = DistanceToColliderXZ(c, p, out _); if (d < bestD) { bestD = d; best = c; isRock = true; } }

            foreach (var c in ShoreZones_Sand)
                if (c) { float d = DistanceToColliderXZ(c, p, out _); if (d < bestD) { bestD = d; best = c; isRock = false; } }

            if (!best) { isRock = true; return p; }

            var b = best.bounds;
            float x = Random.Range(b.min.x, b.max.x);
            float z = Random.Range(b.min.z, b.max.z);
            float y = b.center.y;
            return new Vector3(x, y, z);
        }

        static float DistanceToColliderXZ(Collider c, Vector3 p, out Vector3 nearestXZ)
        {
            var b = c.bounds;
            float x = Mathf.Clamp(p.x, b.min.x, b.max.x);
            float z = Mathf.Clamp(p.z, b.min.z, b.max.z);
            nearestXZ = new Vector3(x, b.center.y, z);

            Vector2 a = new Vector2(p.x, p.z);
            Vector2 q = new Vector2(nearestXZ.x, nearestXZ.z);
            return Vector2.Distance(a, q);
        }

        static float Linear01ToDb(float v01) => (v01 <= 0.0001f) ? -80f : 20f * Mathf.Log10(Mathf.Clamp01(v01));
        static float ClampHz(float hz) => Mathf.Clamp(hz, 20f, 20000f);

        public void EnsurePlaying(SO_AudioCue lightCue, SO_AudioCue medium, SO_AudioCue heavy, SO_AudioCue distant)
        {
            if (ServiceLocator.Audio == null) return;
            loopWashLight = ServiceLocator.Audio.PlayAt(lightCue, Vector3.zero);

            loopWashMed = ServiceLocator.Audio.PlayAt(medium, Vector3.zero);
            loopWashHeavy = ServiceLocator.Audio.PlayAt(heavy, Vector3.zero);
            loopDistant = ServiceLocator.Audio.PlayAt(distant, Vector3.zero);
        }
    }

    static class VecExt
    {
        public static float DistanceXZ(this Vector3 a, Vector3 b)
            => Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));
    }
}
