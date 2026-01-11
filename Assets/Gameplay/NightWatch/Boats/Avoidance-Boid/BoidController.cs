using System.Collections;
using UnityEngine;

namespace LightHouse.Features.Boats
{
    public class BoidController : MonoBehaviour
    {
        // --- COMPONENTS ---
        [Header(" --- COMPONENTS --- ")]
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private GameObject[] _buoyancys;
        private VectorPath _path;
        public VectorPath Path => _path;

        // --- NAVIGATION ---
        [Header(" --- NAVIGATION --- ")]
        [SerializeField] private float _moveForce = 5f;

        // Rotation Y uniquement (sans torque)
        [Header(" --- YAW KINEMATIQUE --- ")]
        [SerializeField] private float _maxYawDegPerSec = 45f;   // vitesse de rotation max (°/s)
        [SerializeField] private float _yawDeadZoneDeg = 0.25f;  // zone morte pour éviter le jitter

        private Vector3 _targetPosition;
        private Vector3 _currentDirection;
        private int _currentPathIndex = 0;

        [SerializeField] private float _pathPointThreshold = 5f;

        [SerializeField, Range(0f, 1f)] private float _progress = 0f;
        public float Progress => _progress;

        // Progress le long de la polyline
        private float[] _cumLengths;
        private float _totalLength;

        public float MoveForce
        {
            get { return _moveForce; }
            set { _moveForce = value; }
        }

        // === Initialization ===
        public void Initialize(VectorPath path)
        {
            _path = path;

            foreach (var buoy in _buoyancys) buoy.gameObject.SetActive(false);
            _rb.isKinematic = true;

            if (_path == null || _path.Paths == null || _path.Paths.Length == 0)
            {
                Debug.LogWarning("BoidController: path vide.");
                enabled = false;
                return;
            }

            // Spawn au point 0, cible = point 1 si dispo
            _currentPathIndex = Mathf.Min(1, _path.Paths.Length - 1);
            _rb.position = _path.Paths[0];
            _targetPosition = _path.Paths[_currentPathIndex];

            // Orientation initiale vers la cible
            var initDir = (_targetPosition - _rb.position); initDir.y = 0f;
            if (initDir.sqrMagnitude > 0.001f)
                _rb.rotation = Quaternion.LookRotation(initDir.normalized, Vector3.up);

            PrecomputePolylineLengths();
            StartCoroutine(EnablePhysics());
            _progress = 0f;
        }

        protected IEnumerator EnablePhysics()
        {
            yield return new WaitForFixedUpdate();

            _rb.isKinematic = false;

            // On laisse le roulis/tangage libres: gérés par les floateurs
            _rb.constraints = RigidbodyConstraints.None;

            // Garde-fous raisonnables
            _rb.maxAngularVelocity = 3.0f;
            _rb.angularDamping = Mathf.Max(0.2f, _rb.angularDamping);

            foreach (var buoy in _buoyancys) buoy.gameObject.SetActive(true);
        }

        // === FixedUpdate ===
        protected virtual void FixedUpdate()
        {
            if (_path.Paths == null || _path.Paths.Length == 0) return;
            if (_currentPathIndex >= _path.Paths.Length) return;

            Vector3 wp = _path.Paths[_currentPathIndex];
            Vector3 prev = _currentPathIndex > 0 ? _path.Paths[_currentPathIndex - 1] : _rb.position;

            Vector3 toTarget = wp - _rb.position; toTarget.y = 0f;
            Vector3 seg = wp - prev; seg.y = 0f;

            bool closeEnough = toTarget.magnitude <= _pathPointThreshold;
            bool passed = Vector3.Dot(seg, toTarget) < 0f;

            if (closeEnough || passed)
            {
                AdvanceToNextPoint();
                return;
            }

            // Direction désirée (sans évitement)
            Vector3 desiredDir = toTarget.sqrMagnitude > 1e-6f ? toTarget.normalized : transform.forward;
            _currentDirection = desiredDir;

            // Rotation yaw sans physique (pas de torque)
            YawRotateTowards(_currentDirection);

            // Propulsion: pousse dans l'avant actuel du bateau (évite le "strafe")
            _rb.AddForce(_rb.transform.forward * _moveForce, ForceMode.Force);

            // Progress polyline
            UpdateProgressAlongPath();
        }

        private void AdvanceToNextPoint()
        {
            _currentPathIndex++;

            if (_currentPathIndex >= _path.Paths.Length)
            {
                OnPathComplete();
                return;
            }

            _targetPosition = _path.Paths[_currentPathIndex];
        }

        private void OnPathComplete()
        {
            Debug.Log($"{gameObject.name} has completed the path.");
            Destroy(this.gameObject);
        }

        // === Progress Polyline ===
        private void PrecomputePolylineLengths()
        {
            int n = _path.Paths.Length;
            _cumLengths = new float[n];
            _cumLengths[0] = 0f;

            float acc = 0f;
            for (int i = 1; i < n; i++)
            {
                float segLen = Vector3.Distance(Flatten(_path.Paths[i - 1]), Flatten(_path.Paths[i]));
                acc += segLen;
                _cumLengths[i] = acc;
            }
            _totalLength = Mathf.Max(0.0001f, acc);
        }

        private static Vector3 Flatten(Vector3 v) { v.y = 0f; return v; }

        protected virtual void UpdateProgressAlongPath()
        {
            if (_cumLengths == null || _cumLengths.Length < 2) { _progress = 0f; return; }

            int i = Mathf.Clamp(_currentPathIndex, 1, _path.Paths.Length - 1);

            Vector3 a = Flatten(_path.Paths[i - 1]);
            Vector3 b = Flatten(_path.Paths[i]);
            Vector3 p = Flatten(_rb.position);

            Vector3 ab = b - a;
            float abLen = ab.magnitude;
            if (abLen < 1e-3f) { _progress = _cumLengths[i] / _totalLength; return; }

            float t = Mathf.Clamp01(Vector3.Dot(p - a, ab / abLen) / abLen);
            float along = _cumLengths[i - 1] + t * abLen;

            _progress = Mathf.Clamp01(along / _totalLength);
        }

        // === Yaw uniquement via MoveRotation ===
        protected virtual void YawRotateTowards(Vector3 desiredDir)
        {
            // Yaw cible depuis la direction désirée (plan XZ)
            desiredDir.y = 0f;
            if (desiredDir.sqrMagnitude < 1e-6f) return;
            desiredDir.Normalize();

            float targetYawDeg = Mathf.Atan2(desiredDir.x, desiredDir.z) * Mathf.Rad2Deg;

            // Extraire l'euler actuel pour préserver le roll/pitch générés par l'eau
            Vector3 e = _rb.rotation.eulerAngles;
            float currentYawDeg = e.y;

            // Delta le plus court et clamp par vitesse max
            float deltaYaw = Mathf.DeltaAngle(currentYawDeg, targetYawDeg);
            if (Mathf.Abs(deltaYaw) <= _yawDeadZoneDeg) return;

            float maxStep = _maxYawDegPerSec * Time.fixedDeltaTime;
            float step = Mathf.Clamp(deltaYaw, -maxStep, maxStep);

            float newYaw = currentYawDeg + step;

            // Appliquer uniquement Y, conserver X/Z
            Quaternion newRot = Quaternion.Euler(e.x, newYaw, e.z);
            _rb.MoveRotation(newRot);
        }
    }

}
