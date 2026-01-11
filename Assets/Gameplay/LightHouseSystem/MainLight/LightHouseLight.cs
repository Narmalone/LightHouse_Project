using LightHouse.Core.Audio;
using LightHouse.Core.Services;
using UnityEngine;

namespace LightHouse.Features.LightHouse
{
    public class LightHouseLight : MonoBehaviour
    {
        public bool Enable = false;
        [Header("Rotation")]
        [Tooltip("Durée d'un tour complet en secondes")]
        public float rotationPeriod = 20f;

        [Header("Light Flash")]
        public bool flashing = true;

        [Tooltip("Intensité normale (EV100)")]
        [Range(0, 50)] public float NormalLightIntensity = 37f;

        [Tooltip("Intensité maximale (EV100)")]
        [Range(0, 50)] public float MaxLightIntensity = 45f;

        [Tooltip("Forme du cycle d'intensité (0-1)")]
        public AnimationCurve _flashCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        public AudioCue LoopGearCue;

        [Tooltip("Durée d'un cycle de flash en secondes")]
        public float flashDuration = 2f;

        private Light _spot;
        private float _time;

        void Awake()
        {
            _spot = GetComponentInChildren<Light>();
            StopLight();
        }

        private IAudioHandle _currentGearHandler;
        public void StartLight()
        {
            if (Application.isPlaying || ServiceLocator.Audio != null)
                _currentGearHandler = ServiceLocator.Audio.PlayAt(LoopGearCue, this.transform.position);
            Enable = true;
            if (!_spot.isActiveAndEnabled)
                _spot.gameObject.SetActive(true);
        }

        public void StopLight()
        {
            if (_currentGearHandler != null)
            {
                _currentGearHandler.Stop(1.5f);
            }
            Enable = false;
            if (_spot.isActiveAndEnabled)
                _spot.gameObject.SetActive(false);
        }


        void Update()
        {
            if (_spot == null || !Enable) return;

            // Rotation constante du phare
            transform.Rotate(Vector3.up, 360f / rotationPeriod * Time.deltaTime, Space.Self);

            if (flashing)
            {
                _time += Time.deltaTime;
                float t = (_time % flashDuration) / flashDuration; // Normalisation
                float curveValue = _flashCurve.Evaluate(t);

                // Interpolation réaliste entre Normal et Max
                float evValue = Mathf.Lerp(NormalLightIntensity, MaxLightIntensity, curveValue);

                // Conversion EV100 → luminance physique (lux)
                // 2^EV100 * 2.5 est une approximation HDRP correcte
                _spot.intensity = Mathf.Pow(2f, evValue) * 2.5f;
            }
            else
            {
                _spot.intensity = Mathf.Pow(2f, NormalLightIntensity) * 2.5f;
            }
        }
    }
}
