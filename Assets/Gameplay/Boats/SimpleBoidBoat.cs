using LightHouse.Game.WaterExtension;
using System.Collections.Generic;
using UnityEngine;

public class SimpleBoidBoat : MonoBehaviour
{
    [Header(" --- COMPONENTS --- ")]
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private PhysicalBuoyancy[] _buoyancys;
    [SerializeField] private RandomPointOnWaterSurface _randomPointOnWaterSurface;

    [Header(" --- NAVIGATION --- ")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _turnSpeed = 30f;

    [Header(" --- AVOIDANCE --- ")]
    [SerializeField] private LayerMask _obstacleMask;
    [SerializeField] private float _overlapDistance = 25f;
    [SerializeField] private float _boatWidth = 3f;
    [SerializeField] private float _boatLength = 7f;
    [SerializeField] private float _boxCastHeight = 2f;
    [SerializeField] private float _avoidanceAngleIncrement = 15f;
    [SerializeField] private int _maxAvoidanceChecks = 5;

    private Vector3 _targetPosition;
    private Vector3 _currentDirection;

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

    private void Start()
    {
        foreach (var buoy in _buoyancys) buoy.enabled = false;
        _rb.isKinematic = true;

        var (entry, exit) = _randomPointOnWaterSurface.GetEntryExitPoints();
        _rb.position = entry;
        _rb.rotation = Quaternion.LookRotation((exit - entry).normalized, Vector3.up);
        _targetPosition = exit;

        StartCoroutine(EnablePhysics());
    }

    private System.Collections.IEnumerator EnablePhysics()
    {
        yield return new WaitForFixedUpdate();
        _rb.isKinematic = false;
        foreach (var buoy in _buoyancys) buoy.enabled = true;
    }

    private void FixedUpdate()
    {
        Vector3 toTarget = _targetPosition - _rb.position;
        toTarget.y = 0f;
        if (toTarget.magnitude < 1f) return;

        Vector3 desiredDirection = toTarget.normalized;
        _currentDirection = ComputeSafeDirection(desiredDirection);

        SteerAndMove(_currentDirection);
    }

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
                _castDebugs.Add(new CastDebug { center = center, direction = dir, hit = hit, isBest = isBest });
#endif
            }
        }

        return bestDir.normalized;
    }

    private void SteerAndMove(Vector3 direction)
    {
        Quaternion desiredRotation = Quaternion.LookRotation(direction, Vector3.up);
        _rb.MoveRotation(Quaternion.RotateTowards(_rb.rotation, desiredRotation, _turnSpeed * Time.fixedDeltaTime));
        _rb.AddForce(_rb.transform.forward * _moveSpeed, ForceMode.Force);
    }

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
}
