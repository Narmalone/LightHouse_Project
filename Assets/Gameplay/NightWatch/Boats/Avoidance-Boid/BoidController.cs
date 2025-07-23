using LightHouse.Game.WaterExtension;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidController : MonoBehaviour
{
    #region === Components ===

    [Header(" --- COMPONENTS --- ")]
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private GameObject[] _buoyancys;
    private VectorPath _path;

    public VectorPath Path => _path;

    #endregion

    #region === Navigation Settings ===

    [Header(" --- NAVIGATION --- ")]
    [SerializeField] private float _moveForce = 5f;
    [SerializeField] private float _turnForce = 30f;

    private Vector3 _targetPosition;
    private Vector3 _currentDirection;
    private int _currentPathIndex = 0;

    [SerializeField] private float _pathPointThreshold = 5f;

    [SerializeField, Range(0f, 1f)] private float _progress = 0f;
    public float Progress => _progress;

    #endregion

    #region === Avoidance Settings ===

    [Header(" --- AVOIDANCE --- ")]
    [SerializeField] private LayerMask _obstacleMask;
    [SerializeField] private float _overlapDistance = 40f;
    [SerializeField] private float _boatWidth = 25f;
    [SerializeField] private float _boatLength = 40f;
    [SerializeField] private float _boxCastHeight = 50f;
    [SerializeField] private float _avoidanceAngleIncrement = 20f;
    [SerializeField] private int _maxAvoidanceChecks = 8;

#if UNITY_EDITOR
    private struct CastDebug
    {
        public Vector3 center;
        public Vector3 direction;
        public bool hit;
        public bool isBest;
    }

    private List<CastDebug> _castDebugs = new();
#endif

    #endregion

    #region === Initialization ===

    public void Initialize(VectorPath path)
    {
        _path = path;
    }

    private void Start()
    {
        foreach (var buoy in _buoyancys)
            buoy.gameObject.SetActive(false);

        _rb.isKinematic = true;

        _currentPathIndex = 0;

        if (_path.Paths != null && _path.Paths.Length > 0)
        {
            _rb.position = _path.Paths[0];
            _targetPosition = _path.Paths[0];
        }
        else
        {
            _rb.position = _path.EntryPoint;
            _targetPosition = _path.EntryPoint;
        }

        Vector3 initialForward = (_path.ExitPoint - _rb.position).normalized;
        _rb.rotation = Quaternion.LookRotation(initialForward, Vector3.up);

        StartCoroutine(EnablePhysics());
    }

    private IEnumerator EnablePhysics()
    {
        yield return new WaitForFixedUpdate();
        _rb.isKinematic = false;

        foreach (var buoy in _buoyancys)
            buoy.gameObject.SetActive(true);
    }

    #endregion

    #region === Update Logic ===

    private void FixedUpdate()
    {
        if (_path.Paths == null || _path.Paths.Length == 0 || _currentPathIndex >= _path.Paths.Length)
            return;

        Vector3 toTarget = _targetPosition - _rb.position;
        toTarget.y = 0f;

        if (toTarget.magnitude < _pathPointThreshold)
        {
            AdvanceToNextPoint();
            return;
        }

        Vector3 desiredDirection = toTarget.normalized;
        _currentDirection = ComputeSafeDirection(desiredDirection);

        SteerAndMove(_currentDirection);
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

    private void UpdateProgressAlongPath()
    {
        if (_path == null || _path.Paths == null || _path.Paths.Length < 2)
        {
            _progress = 0f;
            return;
        }

        Vector3 a = _path.EntryPoint;
        Vector3 b = _path.ExitPoint;
        Vector3 p = _rb.position;

        Vector3 ab = b - a;
        Vector3 ap = p - a;

        float abLength = ab.magnitude;
        if (abLength < 0.01f)
        {
            _progress = 0f;
            return;
        }

        float t = Vector3.Dot(ap, ab.normalized) / abLength;
        _progress = Mathf.Clamp01(t);
    }

    #endregion

    #region === Movement & Avoidance ===

    private Vector3 ComputeSafeDirection(Vector3 desiredDir)
    {
#if UNITY_EDITOR
        _castDebugs.Clear();
#endif

        Vector3 origin = _rb.position + Vector3.up * (_boxCastHeight * 0.5f + 0.1f);
        Quaternion baseRot = Quaternion.LookRotation(desiredDir, Vector3.up);

        Vector3 bestDir = desiredDir;
        float bestScore = -Mathf.Infinity;

        for (int i = 0; i <= _maxAvoidanceChecks; i++)
        {
            float angleOffset = i * _avoidanceAngleIncrement;

            foreach (float sign in new float[] { 1f, -1f })
            {
                float angle = angleOffset * sign;
                Quaternion rot = Quaternion.AngleAxis(angle, Vector3.up) * baseRot;
                Vector3 dir = rot * Vector3.forward;

                Vector3 center = origin + dir * _overlapDistance - new Vector3(0f, _boxCastHeight * 0.5f, 0f);
                Vector3 halfExtents = new Vector3(_boatWidth * 0.5f, _boxCastHeight * 0.5f, _boatLength * 0.5f);
                Quaternion rotation = Quaternion.LookRotation(dir, Vector3.up);

                bool hit = Physics.OverlapBox(center, halfExtents, rotation, _obstacleMask).Length > 0;
                float score = hit ? 0f : Vector3.Dot(desiredDir, dir);

                bool isBest = false;
                if (score > bestScore)
                {
                    bestScore = score;
                    bestDir = dir;
                    isBest = true;
                }

#if UNITY_EDITOR
                _castDebugs.Add(new CastDebug
                {
                    center = center,
                    direction = dir,
                    hit = hit,
                    isBest = isBest
                });
#endif
            }
        }

        return bestDir.normalized;
    }

    private void SteerAndMove(Vector3 direction)
    {
        Quaternion desiredRotation = Quaternion.LookRotation(direction, Vector3.up);
        Vector3 torque = Vector3.Cross(_rb.transform.forward, direction) * _turnForce;
        _rb.AddTorque(torque, ForceMode.Force);
        _rb.AddForce(_rb.transform.forward * _moveForce, ForceMode.Force);
    }

    #endregion

    #region === Debug & Gizmos ===

    private void OnDrawGizmosSelected()
    {
        if (_rb == null) return;

#if UNITY_EDITOR
        foreach (var d in _castDebugs)
        {
            Gizmos.color = d.isBest ? Color.green : d.hit ? Color.red : Color.cyan;

            Vector3 halfExtents = new Vector3(_boatWidth * 0.5f, _boxCastHeight * 0.5f, _boatLength * 0.5f);
            Quaternion rot = Quaternion.LookRotation(d.direction);
            Matrix4x4 matrix = Matrix4x4.TRS(d.center, rot, Vector3.one);
            Gizmos.matrix = matrix;
            Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2);
        }
#endif
    }

    #endregion
}
