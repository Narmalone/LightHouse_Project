using KinematicCharacterController;
using LightHouse.Core.Utilities;
using System;
using UnityEngine;


namespace LightHouse.Features.Boats
{
    /// <summary>
    /// Bateau cinématique qui suit un VectorPath point par point,
    /// compatible KCC via PhysicsMover. Pas de AddForce, pas de rigidbody dynamique.
    /// </summary>
    public class BoatPathMover : MonoBehaviour, IMoverController
    {
        #region Events
        public event Action OnPathCompleted;
        #endregion

        #region Serialized Fields

        [Header("KCC Mover")]
        [SerializeField] private PhysicsMover _mover;

        [Header("Path / Navigation")]
        [SerializeField] private VectorPath _path;
        [SerializeField] private int _currentPathIndex = 0;

        [Tooltip("Distance pour considérer un waypoint atteint et passer au suivant")]
        [SerializeField] private float _waypointReachDistance = 5f;

        [Tooltip("Vitesse linéaire (m/s) le long du path")]
        [SerializeField] private float _baseMoveSpeed = 5f;
        [SerializeField] private float _moveSpeed = 5f;

        [Header("Rotation (Yaw uniquement)")]
        [SerializeField] private float _maxYawDegPerSec = 45f;
        [SerializeField] private float _yawDeadZoneDeg = 0.25f;

        [Header("Mer / Tangage")]
        [SerializeField] private FloaterGetterController _floater;
        [SerializeField] private float _waterTiltLerp = 5f;
        [SerializeField] private float _waterHeightLerp = 5f;
        [SerializeField] private float _waterHeightOffset = 0f;

        [Header("Debug")]
        [SerializeField, Range(0f, 1f)] private float _progress01 = 0f;

        #endregion

        #region Public API / Properties

        public float Speed { get => _moveSpeed; set => _moveSpeed = value; }
        public float BaseMoveSpeed => _baseMoveSpeed;
        public float Progress => _progress01;
        public bool IsPathCompleted { get; private set; }

        #endregion

        #region Private State

        private float _pitchDeg;
        private float _rollDeg;
        private float _currentSeaHeight;

        private float[] _cumLengths;
        private float _totalLength;

        private Vector3 _currentPos;
        private Quaternion _currentRot;
        private Vector3 _velocity;
        private float _currentYawDeg;
        private bool _initialized;
        private bool _pathCompletedInvoked;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_mover == null)
                _mover = GetComponent<PhysicsMover>();

            if (_mover != null)
                _mover.MoverController = this;
        }

        #endregion

        #region Initialization

        public void InitializeOnPath()
        {
            if (_path == null || _path.Paths == null || _path.Paths.Length == 0)
            {
                Debug.LogWarning($"{name}: BoatPathMover - Path vide, désactivation.");
                enabled = false;
                return;
            }

            Vector3 startPos = _path.Paths[0];
            _currentPathIndex = Mathf.Min(1, _path.Paths.Length - 1);

            Vector3 dir = (_path.Paths[_currentPathIndex] - startPos);
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f)
                dir = transform.forward;

            Quaternion startRot = Quaternion.LookRotation(dir.normalized, Vector3.up);

            _currentPos = startPos;
            _currentRot = startRot;
            _currentYawDeg = _currentRot.eulerAngles.y;
            _pitchDeg = 0f;
            _rollDeg = 0f;
            _currentSeaHeight = _currentPos.y;

            _mover.SetPositionAndRotation(_currentPos, _currentRot);
            PrecomputePolylineLengths();

            _initialized = true;
            IsPathCompleted = false;
            _pathCompletedInvoked = false;
            _progress01 = 0f;
        }

        #endregion

        #region KCC Mover Controller

        public void UpdateMovement(out Vector3 goalPosition, out Quaternion goalRotation, float deltaTime)
        {
            goalPosition = _currentPos;
            goalRotation = _currentRot;

            if (!_initialized || _path == null || _path.Paths == null || _path.Paths.Length == 0)
                return;

            // 🚫 Sécurité : si on a déjà fini le chemin, ne pas lire au-delà du tableau
            if (IsPathCompleted || _currentPathIndex >= _path.Paths.Length)
            {
                _currentPathIndex = Mathf.Clamp(_currentPathIndex, 0, _path.Paths.Length - 1);
                goalPosition = _currentPos;
                goalRotation = _currentRot;
                return;
            }

            // -------------------------------------------------
            // 1. Avancer le long du path
            // -------------------------------------------------
            Vector3 targetWp = _path.Paths[_currentPathIndex];
            Vector3 flatToTarget = targetWp - _currentPos;
            flatToTarget.y = 0f;

            float distToTarget = flatToTarget.magnitude;
            float stepDist = _moveSpeed * deltaTime;

            if (distToTarget <= _waypointReachDistance || stepDist >= distToTarget)
            {
                _currentPos = targetWp;
                _currentPathIndex++;

                if (_currentPathIndex >= _path.Paths.Length)
                {
                    // ✅ Fin du path atteinte
                    _currentPathIndex = _path.Paths.Length - 1;
                    UpdateProgressAlongPath();

                    IsPathCompleted = true;
                    if (!_pathCompletedInvoked)
                    {
                        _pathCompletedInvoked = true;
                        OnPathCompleted?.Invoke();
                    }

                    goalPosition = _currentPos;
                    goalRotation = _currentRot;
                    return;
                }
            }
            else
            {
                Vector3 moveDir = flatToTarget.normalized;
                _currentPos += moveDir * stepDist;
            }

            // -------------------------------------------------
            // 2. Mettre à jour la rotation (yaw)
            // -------------------------------------------------
            Vector3 desiredDir = (_path.Paths[_currentPathIndex] - _currentPos);
            desiredDir.y = 0f;

            if (desiredDir.sqrMagnitude < 0.0001f)
            {
                desiredDir = _currentRot * Vector3.forward;
                desiredDir.y = 0f;
                if (desiredDir.sqrMagnitude < 0.0001f)
                    desiredDir = Vector3.forward;
            }
            desiredDir.Normalize();

            float targetYawDeg = Mathf.Atan2(desiredDir.x, desiredDir.z) * Mathf.Rad2Deg;
            float deltaYaw = Mathf.DeltaAngle(_currentYawDeg, targetYawDeg);

            if (Mathf.Abs(deltaYaw) > _yawDeadZoneDeg)
            {
                float maxStep = _maxYawDegPerSec * deltaTime;
                float clampedStep = Mathf.Clamp(deltaYaw, -maxStep, maxStep);
                _currentYawDeg += clampedStep;
            }

            // -------------------------------------------------
            // 3. Effets de la mer
            // -------------------------------------------------
            if (_floater != null)
            {
                float targetSeaHeight = _floater.AverageWaterHeight + _waterHeightOffset;

                _currentSeaHeight = Mathf.Lerp(
                    _currentSeaHeight,
                    targetSeaHeight,
                    1f - Mathf.Exp(-_waterHeightLerp * deltaTime)
                );

                Vector3 seaUp = (_floater.AverageWaterNormal.sqrMagnitude > 0.001f)
                    ? _floater.AverageWaterNormal.normalized
                    : Vector3.up;

                Vector3 fwd = Quaternion.Euler(0f, _currentYawDeg, 0f) * Vector3.forward;
                Vector3 fwdOnPlane = Vector3.ProjectOnPlane(fwd, seaUp).normalized;
                if (fwdOnPlane.sqrMagnitude < 1e-4f)
                    fwdOnPlane = Vector3.forward;

                Quaternion waterAlignRot = Quaternion.LookRotation(fwdOnPlane, seaUp);
                Vector3 waterEul = waterAlignRot.eulerAngles;

                float targetPitch = Normalize180(waterEul.x);
                float targetRoll = Normalize180(waterEul.z);

                _pitchDeg = Mathf.Lerp(_pitchDeg, targetPitch, 1f - Mathf.Exp(-_waterTiltLerp * deltaTime));
                _rollDeg = Mathf.Lerp(_rollDeg, targetRoll, 1f - Mathf.Exp(-_waterTiltLerp * deltaTime));
            }
            else
            {
                _currentSeaHeight = Mathf.Lerp(_currentSeaHeight, _currentPos.y, 1f - Mathf.Exp(-_waterHeightLerp * deltaTime));
                _pitchDeg = Mathf.Lerp(_pitchDeg, 0f, 1f - Mathf.Exp(-_waterTiltLerp * deltaTime));
                _rollDeg = Mathf.Lerp(_rollDeg, 0f, 1f - Mathf.Exp(-_waterTiltLerp * deltaTime));
            }

            // -------------------------------------------------
            // 4. Calcul final
            // -------------------------------------------------
            _currentRot = Quaternion.Euler(_pitchDeg, _currentYawDeg, _rollDeg);
            _currentPos.y = _currentSeaHeight;

            _velocity = (_path.Paths[_currentPathIndex] - _currentPos).normalized * _moveSpeed;
            UpdateProgressAlongPath();

            goalPosition = _currentPos;
            goalRotation = _currentRot;
        }

        #endregion

        #region Math / Progress

        private static float Normalize180(float deg)
        {
            deg %= 360f;
            if (deg > 180f) deg -= 360f;
            return deg;
        }

        private void PrecomputePolylineLengths()
        {
            var pts = _path.Paths;
            int n = pts.Length;
            _cumLengths = new float[n];
            _cumLengths[0] = 0f;

            float acc = 0f;
            for (int i = 1; i < n; i++)
            {
                Vector3 a = pts[i - 1]; a.y = 0f;
                Vector3 b = pts[i]; b.y = 0f;
                float segLen = Vector3.Distance(a, b);
                acc += segLen;
                _cumLengths[i] = acc;
            }
            _totalLength = Mathf.Max(0.0001f, acc);
        }

        private void UpdateProgressAlongPath()
        {
            if (_cumLengths == null || _cumLengths.Length < 2 || _currentPathIndex <= 0)
            {
                _progress01 = IsPathCompleted ? 1f : 0f;
                return;
            }

            int i = Mathf.Clamp(_currentPathIndex, 1, _path.Paths.Length - 1);

            Vector3 a = _path.Paths[i - 1]; a.y = 0f;
            Vector3 b = _path.Paths[i]; b.y = 0f;
            Vector3 p = _currentPos; p.y = 0f;

            Vector3 ab = b - a;
            float abLen = ab.magnitude;
            if (abLen < 1e-4f)
            {
                _progress01 = _cumLengths[i] / _totalLength;
                return;
            }

            float t = Mathf.Clamp01(Vector3.Dot(p - a, ab / abLen) / abLen);
            float along = _cumLengths[i - 1] + t * abLen;

            _progress01 = Mathf.Clamp01(along / _totalLength);
        }

        #endregion

        #region Helpers

        public void ResetToStart() => InitializeOnPath();

        public void CompletePathNow()
        {
            if (IsPathCompleted) return;

            _currentPathIndex = (_path != null && _path.Paths != null) ? _path.Paths.Length - 1 : 0;
            UpdateProgressAlongPath();

            IsPathCompleted = true;
            if (!_pathCompletedInvoked)
            {
                _pathCompletedInvoked = true;
                OnPathCompleted?.Invoke();
            }
        }

        #endregion

        #region Gizmos

        private void OnDrawGizmosSelected()
        {
            if (_path != null && _path.Paths != null && _path.Paths.Length > 0)
            {
                Gizmos.color = Color.cyan;
                for (int i = 0; i < _path.Paths.Length; i++)
                {
                    Gizmos.DrawSphere(_path.Paths[i], 0.3f);
                    if (i < _path.Paths.Length - 1)
                        Gizmos.DrawLine(_path.Paths[i], _path.Paths[i + 1]);
                }

                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(_currentPos, 0.4f);
                Gizmos.DrawRay(_currentPos, (_currentRot * Vector3.forward) * 2f);
            }
        }

        #endregion
    }

}
