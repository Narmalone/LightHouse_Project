using LightHouse.Game.WaterExtension;
using UnityEngine;

public class SimpleBoidBoat : MonoBehaviour
{
    [Header(" --- COMPONENTS --- ")]
    [SerializeField] private Rigidbody _rb;
    public Transform RaycastsTransform;
    [SerializeField] private PhysicalBuoyancy[] _buoyancys;
    [SerializeField] private RandomPointOnWaterSurface _randomPointOnWaterSurface;

    [Header(" --- NAVIGATION --- ")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _turnSpeed = 30.0f;

    [Header(" --- OBSTACLE AVOIDANCE --- ")]
    [SerializeField] private LayerMask _obstacleMasks;
    [SerializeField] private float _obstacleCheckDistance = 50.0f;
    [SerializeField] private int _avoidanceRayCount = 7;
    [SerializeField] private float _avoidanceSpread = 60f;
    [SerializeField, Range(0f, 1f)] private float _avoidanceWeight = 0.6f;  // importance de l'évitement
    [SerializeField] private float _avoidanceSmoothTime = 0.25f;

    [Header(" --- DEBUG --- ")]
    private Vector3 _lastFinalDirection = Vector3.forward;
    private bool _lastObstacleBetween = false;

    private Vector3 _smoothedAvoidance = Vector3.zero;

    private void Start()
    {
        foreach (var buoy in _buoyancys) buoy.enabled = false;
        _rb.isKinematic = true;

        var (entry, exit) = _randomPointOnWaterSurface.GetEntryExitPoints();
        _rb.position = entry;
        _rb.rotation = Quaternion.LookRotation((exit - entry).normalized, Vector3.up);

        StartCoroutine(EnablePhysicsAndBuoyancyNextFrame());
    }

    private System.Collections.IEnumerator EnablePhysicsAndBuoyancyNextFrame()
    {
        yield return new WaitForFixedUpdate();
        _rb.isKinematic = false;
        foreach (var buoy in _buoyancys) buoy.enabled = true;
    }

    private void FixedUpdate()
    {
        if (_randomPointOnWaterSurface.Destination == null) return;

        Vector3 toTarget = _randomPointOnWaterSurface.Destination - _rb.position;
        toTarget.y = 0f;
        if (toTarget.magnitude < 0.1f) return;

        Vector3 directionToTarget = toTarget.normalized;
        Vector3 avoidance = ComputeAvoidanceDirection();

        // Smooth l’évitement pour éviter les changements trop brusques
        _smoothedAvoidance = Vector3.Lerp(_smoothedAvoidance, avoidance, Time.fixedDeltaTime / _avoidanceSmoothTime);

        Vector3 finalDir = ((1f - _avoidanceWeight) * directionToTarget + _avoidanceWeight * _smoothedAvoidance).normalized;

        _lastFinalDirection = finalDir;
        _lastObstacleBetween = avoidance != Vector3.zero;

        SteerAndMove(finalDir);
    }

    private Vector3 ComputeAvoidanceDirection()
    {
        Vector3 avoidance = Vector3.zero;
        Vector3 origin = RaycastsTransform.position;
        Vector3 forward = RaycastsTransform.transform.forward;
        float halfSpread = _avoidanceSpread * 0.5f;

        for (int i = 0; i < _avoidanceRayCount; i++)
        {
            float t = (_avoidanceRayCount == 1) ? 0.5f : (i / (float)(_avoidanceRayCount - 1));
            float angle = Mathf.Lerp(-halfSpread, halfSpread, t);
            Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * forward;

            if (Physics.Raycast(origin, dir, out RaycastHit hit, _obstacleCheckDistance, _obstacleMasks))
            {
                float weight = 1f - (hit.distance / _obstacleCheckDistance);
                avoidance -= dir * weight;
            }
        }

        return avoidance;
    }

    private void SteerAndMove(Vector3 direction)
    {
        if (direction == Vector3.zero) return;

        Quaternion desiredRotation = Quaternion.LookRotation(direction, Vector3.up);
        _rb.MoveRotation(Quaternion.RotateTowards(_rb.rotation, desiredRotation, _turnSpeed * Time.fixedDeltaTime));
        _rb.AddForce(_rb.transform.forward * _moveSpeed, ForceMode.Force);
    }

    private void OnDrawGizmosSelected()
    {
        if (_rb == null || _randomPointOnWaterSurface == null) return;

        Vector3 origin = RaycastsTransform.position;
        Vector3 toTarget = _randomPointOnWaterSurface.Destination - origin;
        toTarget.y = 0f;
        Vector3 dirToTarget = toTarget.normalized;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(origin, _randomPointOnWaterSurface.Destination);

        Gizmos.color = _lastObstacleBetween ? Color.magenta : Color.cyan;
        Gizmos.DrawRay(origin, dirToTarget * _obstacleCheckDistance);

        float halfSpread = _avoidanceSpread * 0.5f;
        for (int i = 0; i < _avoidanceRayCount; i++)
        {
            float t = (_avoidanceRayCount == 1) ? 0.5f : (i / (float)(_avoidanceRayCount - 1));
            float angle = Mathf.Lerp(-halfSpread, halfSpread, t);
            Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * RaycastsTransform.transform.forward;

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(origin, dir * _obstacleCheckDistance);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawRay(origin, _lastFinalDirection * 5f);
    }
}
