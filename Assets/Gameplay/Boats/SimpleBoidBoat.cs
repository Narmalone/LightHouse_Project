using LightHouse.Game.WaterExtension;
using UnityEngine;

public class SimpleBoidBoat : MonoBehaviour
{
    [Header(" --- COMPONENTS --- ")]
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Transform RaycastsTransform;
    [SerializeField] private PhysicalBuoyancy[] _buoyancys;
    [SerializeField] private RandomPointOnWaterSurface _randomPointOnWaterSurface;

    [Header(" --- NAVIGATION --- ")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _turnSpeed = 30.0f;

    [Header(" --- OBSTACLE AVOIDANCE --- ")]
    [SerializeField] private LayerMask _obstacleMasks;
    [SerializeField] private float _rayDistance = 50f;
    [SerializeField] private int _rayCount = 9;
    [SerializeField] private float _raySpread = 90f;
    [SerializeField] private float _repathDelay = 0.25f;
    [SerializeField] private float _maxAngleChange = 45f;

    private Vector3 _targetPosition;
    private Vector3 _currentAvoidanceDirection = Vector3.zero;
    private float _timeSinceLastRaycast = 0f;
    private Vector3 _lastFinalDirection = Vector3.forward;

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
        if (_targetPosition == null) return;

        Vector3 toTarget = _targetPosition - _rb.position;
        toTarget.y = 0f;
        if (toTarget.magnitude < 1f) return;

        Vector3 dirToTarget = toTarget.normalized;

        _timeSinceLastRaycast += Time.fixedDeltaTime;
        if (_timeSinceLastRaycast >= _repathDelay)
        {
            _timeSinceLastRaycast = 0f;
            _currentAvoidanceDirection = ComputeSafeDirection(dirToTarget);
        }

        Vector3 finalDir = Vector3.RotateTowards(_rb.transform.forward, _currentAvoidanceDirection, Mathf.Deg2Rad * _maxAngleChange, 0f).normalized;
        _lastFinalDirection = finalDir;

        SteerAndMove(finalDir);
    }

    private Vector3 ComputeSafeDirection(Vector3 desired)
    {
        Vector3 origin = RaycastsTransform.position;
        float halfSpread = _raySpread * 0.5f;

        Vector3 bestDir = desired;
        float bestScore = float.MinValue;

        for (int i = 0; i < _rayCount; i++)
        {
            float t = (_rayCount == 1) ? 0.5f : i / (float)(_rayCount - 1);
            float angle = Mathf.Lerp(-halfSpread, halfSpread, t);
            Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * desired;

            bool hit = Physics.Raycast(origin, dir, _rayDistance, _obstacleMasks);
            if (!hit)
            {
                float dot = Vector3.Dot(desired, dir); // +1 = aligné, -1 = opposé
                if (dot > bestScore)
                {
                    bestScore = dot;
                    bestDir = dir;
                }
            }
        }

        return bestDir.normalized;
    }

    private void SteerAndMove(Vector3 direction)
    {
        if (direction == Vector3.zero) return;

        Quaternion targetRot = Quaternion.LookRotation(direction, Vector3.up);
        _rb.MoveRotation(Quaternion.RotateTowards(_rb.rotation, targetRot, _turnSpeed * Time.fixedDeltaTime));
        _rb.AddForce(_rb.transform.forward * _moveSpeed, ForceMode.Force);
    }

    private void OnDrawGizmosSelected()
    {
        if (_rb == null || RaycastsTransform == null) return;

        Vector3 origin = RaycastsTransform.position;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(origin, _targetPosition);

        float halfSpread = _raySpread * 0.5f;
        Vector3 forward = (_targetPosition - _rb.position).normalized;
        for (int i = 0; i < _rayCount; i++)
        {
            float t = (_rayCount == 1) ? 0.5f : i / (float)(_rayCount - 1);
            float angle = Mathf.Lerp(-halfSpread, halfSpread, t);
            Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * forward;

            Gizmos.color = Physics.Raycast(origin, dir, _rayDistance, _obstacleMasks) ? Color.magenta : Color.yellow;
            Gizmos.DrawRay(origin, dir * _rayDistance);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawRay(origin, _lastFinalDirection * 10f);
    }
}
