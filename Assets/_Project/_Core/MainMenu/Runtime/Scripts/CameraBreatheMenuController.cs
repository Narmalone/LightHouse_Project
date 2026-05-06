using UnityEngine;

namespace LightHouse.Features.Menu.Camera
{
    /// <summary>
    /// Ajoute un mouvement organique à la caméra (respiration + bruit).
    /// </summary>
    public class CameraBreatheMenuController : MonoBehaviour
    {
        #region ===== Settings =====

        [Header("Breathing")]
        [SerializeField] private float _amplitude = 0.015f;
        [SerializeField] private float _frequency = 1.0f;

        [Header("Noise")]
        [SerializeField] private float _noiseSpeed = 0.5f;
        [SerializeField] private float _noiseAmount = 0.5f;

        [Header("Rotation")]
        [SerializeField] private float _rotX = 0.3f;
        [SerializeField] private float _rotY = 0.2f;

        #endregion

        #region ===== State =====

        private Vector3 _startPosition;
        private Quaternion _startRotation;

        #endregion

        #region ===== Unity Lifecycle =====

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            UpdateMotion(Time.time);
        }

        #endregion

        #region ===== Initialization =====

        private void Initialize()
        {
            _startPosition = transform.localPosition;
            _startRotation = transform.localRotation;
        }

        #endregion

        #region ===== Motion =====

        private void UpdateMotion(float time)
        {
            float noise = ComputeNoise(time);

            Vector3 positionOffset = ComputePositionOffset(time, noise);
            Quaternion rotationOffset = ComputeRotationOffset(time, noise);

            ApplyTransform(positionOffset, rotationOffset);
        }

        private float ComputeNoise(float time)
        {
            return Mathf.PerlinNoise(time * _noiseSpeed, 0f) * _noiseAmount;
        }

        private Vector3 ComputePositionOffset(float time, float noise)
        {
            float primaryBreath = Mathf.Sin(time * _frequency + noise) * _amplitude;
            float secondaryBreath = Mathf.Sin(time * (_frequency * 0.5f)) * (_amplitude * 0.3f);

            return new Vector3(
                Mathf.Sin(time * 0.3f) * 0.002f,
                primaryBreath + secondaryBreath,
                Mathf.Cos(time * 0.2f) * 0.002f
            );
        }

        private Quaternion ComputeRotationOffset(float time, float noise)
        {
            float rotOffsetX = Mathf.Sin(time * _frequency + noise) * _rotX;
            float rotOffsetY = Mathf.Cos(time * 0.7f) * _rotY;

            return Quaternion.Euler(rotOffsetX, rotOffsetY, 0f);
        }

        private void ApplyTransform(Vector3 positionOffset, Quaternion rotationOffset)
        {
            transform.localPosition = _startPosition + positionOffset;
            transform.localRotation = _startRotation * rotationOffset;
        }

        #endregion
    }
}