using Cinemachine;
using LightHouse.Core.Extensions;
using UnityEngine;

namespace LightHouse.Core.Player
{
    [System.Serializable]
    public struct CameraInput
    {
        public Vector2 Look;
    }

    public class PlayerCamera : MonoBehaviour
    {
        [Header("Rotation")]
        public bool InvertX = false;
        public bool InvertY = false;

        [Range(-90f, 90f)] public float DefaultVerticalAngle = 20f;
        [Range(-90f, 90f)] public float MinVerticalAngle = -90f;
        [Range(-90f, 90f)] public float MaxVerticalAngle = 90f;

        [Tooltip("Vitesse d'application du look input en °/unité d'input")]
        public float RotationSpeed = 1f;

        [Header("Sensitivity")]
        public float SensiX = 1f;
        public float SensiY = 1f;

        [Header("FOV")]
        public float FOV = 80f;
        public float DefaultFOV = 90f;

        [Header("Référence d'orientation")]
        [Tooltip("false = utiliser Vector3.up (monde stable, anti-gerbe)\ntrue  = utiliser FollowTransform.up (coller à l'animation tête)")]
        public bool UseFollowUpTransform = false;

        [Header("Lean caméra (roll auto)")]
        [Tooltip("Amplitude max du tilt latéral en degrés (ex: 5). 0 = aucune inclinaison.")]
        public float MaxLeanAngle = 5f;

        [Tooltip("Réactivité du lean. Plus c'est haut, plus ça colle vite au mouvement.")]
        public float LeanSharpness = 12f;

        // runtime lean state
        private float _currentLeanAngle; // ce qu'on applique réellement
        private float _leanVelocity;     // si tu veux passer à SmoothDamp plus tard

        public Transform Transform { get; private set; }
        [field: SerializeField] public Transform FollowTransform { get; private set; }

        // direction 'avant' sur le plan horizontal
        public Vector3 PlanarDirection { get; set; }

        [SerializeField] private CinemachineVirtualCamera _cm;
        public CinemachineVirtualCamera CinemachineCamera => _cm;

        [SerializeField] private Camera _playerCamera;
        public Camera Camera => _playerCamera;

        private float _targetVerticalAngle;

        void OnValidate()
        {
            DefaultVerticalAngle = Mathf.Clamp(DefaultVerticalAngle, MinVerticalAngle, MaxVerticalAngle);
            if (_cm != null)
                _cm.m_Lens.FieldOfView = FOV;
        }

        void Awake()
        {
            Transform = this.transform;
            _targetVerticalAngle = 0f;
            PlanarDirection = Vector3.forward;
        }

        private void Start()
        {
            if (FollowTransform != null)
            {
                // init direction avant juste pour le premier frame
                PlanarDirection = FollowTransform.forward;
                Transform.position = FollowTransform.position;
            }

            if (_playerCamera == null)
                _playerCamera = Camera.main;
        }

        public void SetFov(float fov)
        {
            if (_cm != null)
                _cm.SetFieldOfView(fov);
            if (_playerCamera != null)
                _playerCamera.fieldOfView = fov;
        }

        /// <param name="rotationInput">x = yaw input, y = pitch input (genre souris)</param>
        /// <param name="moveInput">x = strafe (-1 gauche / +1 droite), y = forward/back (1 avant / -1 arrière)</param>
        public void UpdateWithInput(float deltaTime, Vector2 rotationInput, Vector2 moveInput)
        {
            if (!FollowTransform)
                return;

            // --- 1. Traiter l'input look ---
            float yawInput = rotationInput.x * SensiX * (InvertX ? -1f : 1f);
            float pitchInput = rotationInput.y * SensiY * (InvertY ? 1f : -1f);

            // up de référence yaw
            Vector3 upRef = UseFollowUpTransform ? FollowTransform.up : Vector3.up;

            // (A) YAW accumulé autour de upRef
            Quaternion yawDelta = Quaternion.AngleAxis(yawInput * RotationSpeed, upRef);
            PlanarDirection = yawDelta * PlanarDirection;

            // reprojeter PlanarDirection sur le plan perpendiculaire à upRef
            PlanarDirection = Vector3.Cross(
                upRef,
                Vector3.Cross(PlanarDirection, upRef)
            ).normalized;

            Quaternion yawRot = Quaternion.LookRotation(PlanarDirection, upRef);

            // (B) PITCH contrôlé par le joueur
            _targetVerticalAngle -= (pitchInput * RotationSpeed);
            _targetVerticalAngle = Mathf.Clamp(_targetVerticalAngle, MinVerticalAngle, MaxVerticalAngle);

            Quaternion pitchRot = Quaternion.Euler(_targetVerticalAngle, 0f, 0f);

            // Base rot sans lean
            Quaternion baseRot = yawRot * pitchRot;

            // --- 2. Calculer le lean cible à partir du déplacement latéral ---
            // moveInput.x <0 => gauche ; >0 => droite
            // On veut incliner la tête dans le sens du mouvement => petit roll
            float strafe = Mathf.Clamp(moveInput.x, -1f, 1f);

            float desiredLeanAngle = 0f;
            if (MaxLeanAngle > 0f)
            {
                // convention : pencher vers l'intérieur du virage
                // On peut décider du signe. On essaye :
                // strafe droite ( +1 ) => caméra penche légèrement à droite (angle négatif)
                desiredLeanAngle = -strafe * MaxLeanAngle;
            }

            // Lissage du lean (expo, framerate-indep)
            {
                // sharpness haut => on colle vite la cible
                float t = 1f - Mathf.Exp(-LeanSharpness * deltaTime);
                _currentLeanAngle = Mathf.Lerp(_currentLeanAngle, desiredLeanAngle, t);
            }

            // --- 3. Injecter le lean (roll autour de l'axe forward de la cam) ---
            Quaternion rollRot = Quaternion.AngleAxis(_currentLeanAngle, baseRot * Vector3.forward);

            Quaternion finalRot = rollRot * baseRot;

            // --- 4. Appliquer la rotation finale ---
            Transform.rotation = finalRot;

            // --- 5. Position snap sur la tête ---
            Transform.position = FollowTransform.position;
        }

        public void SetRotation(Quaternion rotation)
        {
            Transform.rotation = rotation;

            Vector3 forward = rotation * Vector3.forward;
            forward.y = 0f;

            if (forward.sqrMagnitude > 0.001f)
                PlanarDirection = forward.normalized;

            _targetVerticalAngle = rotation.eulerAngles.x;
        }
    }
}
