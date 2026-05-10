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

        public SO_AudioCue LoopGearCue;

        [Tooltip("Durée d'un cycle de flash en secondes")]
        public float flashDuration = 2f;

        [SerializeField] private Light _spot;
        private float _time;

        private IAudioHandle _currentGearHandler;
        public void StartLight()
        {
            Enable = true;

            if (_spot != null && !_spot.gameObject.activeSelf)
                _spot.gameObject.SetActive(true);

            if (Application.isPlaying && ServiceLocator.Audio != null && LoopGearCue != null)
                _currentGearHandler = ServiceLocator.Audio.PlayAt(LoopGearCue, transform.position);
        }

        public void StopLight()
        {
            Enable = false;

            if (_currentGearHandler != null)
            {
                _currentGearHandler.Stop(1.5f);
                _currentGearHandler = null;
            }

            if (_spot != null && _spot.gameObject.activeSelf)
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
