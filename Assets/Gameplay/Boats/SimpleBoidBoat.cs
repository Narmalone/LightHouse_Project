using LightHouse.Game.WaterExtension;
using UnityEngine;

public class SimpleBoidBoat : MonoBehaviour
{
    [Header(" --- COMPONENTS --- ")]
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private PhysicalBuoyancy[] _buoyancys;
    [SerializeField] private RandomPointOnWaterSurface _randomPointOnWaterSurface;

    [Header(" --- NAVIGATION --- ")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _turnSpeed = 30.0f;

    [Header("--- OBSTACLE AVOIDANCE --- ")]
    [SerializeField] private LayerMask _obstacleMasks;
    [SerializeField] private float _obstacleCheckDistance = 50.0f;
    [SerializeField] private float _avoidanceAngle = 60f;
    [SerializeField] private float _resumeTime = 1.5f;

    [Header(" --- DEBUG --- ")]
    [SerializeField] private bool _isAvoiding = false;
    private float _clearTimer = 0f;
    private Vector3 _currentAvoidanceDirection;
    private enum AvoidSide { Left, Right }
    private AvoidSide _avoidanceSide;

    private Vector3 _lastFinalDirection = Vector3.forward;
    private bool _lastObstacleBetween = false;


    private void Start()
    {
        // 1. Désactiver les flottabilités
        foreach (var buoy in _buoyancys)
        {
            buoy.enabled = false;
        }

        // 2. Désactiver la physique du Rigidbody temporairement
        _rb.isKinematic = true;

        // 3. Positionner et orienter
        var (entry, exit) = _randomPointOnWaterSurface.GetEntryExitPoints();
        _rb.position = entry;
        _rb.rotation = Quaternion.LookRotation((exit - entry).normalized, Vector3.up);

        // 4. Réactiver les forces physiques au frame suivant
        StartCoroutine(EnablePhysicsAndBuoyancyNextFrame());
    }
    private System.Collections.IEnumerator EnablePhysicsAndBuoyancyNextFrame()
    {
        yield return new WaitForFixedUpdate(); // attendre que la physique prenne en compte la nouvelle position

        _rb.isKinematic = false;

        foreach (var buoy in _buoyancys)
        {
            buoy.enabled = true;
        }
    }


    private void FixedUpdate()
    {
        if (_randomPointOnWaterSurface.Destination == null) return;

        Vector3 toTarget = _randomPointOnWaterSurface.Destination - _rb.transform.position;
        toTarget.y = 0f;

        if (toTarget.magnitude < 0.1f) return;

        Vector3 directionToTarget = toTarget.normalized;

        bool obstacleBetween = Physics.Raycast(_rb.transform.position, directionToTarget, out RaycastHit hit, _obstacleCheckDistance, _obstacleMasks);
        _lastObstacleBetween = obstacleBetween;

        if (!_isAvoiding && obstacleBetween)
        {
            _isAvoiding = true;
            _avoidanceSide = (Random.value > 0.5f) ? AvoidSide.Left : AvoidSide.Right;

            float angle = _avoidanceSide == AvoidSide.Left ? -_avoidanceAngle : _avoidanceAngle;
            _currentAvoidanceDirection = Quaternion.AngleAxis(angle, Vector3.up) * directionToTarget;
            _clearTimer = 0f;
        }

        if (_isAvoiding)
        {
            if (!obstacleBetween)
            {
                _clearTimer += Time.fixedDeltaTime;
                if (_clearTimer >= _resumeTime)
                {
                    _isAvoiding = false;
                    _clearTimer = 0f;
                }
            }
            else
            {
                _clearTimer = 0f;
            }
        }

        Vector3 finalDir = _isAvoiding ? _currentAvoidanceDirection : directionToTarget;
        _lastFinalDirection = finalDir.normalized;

        SteerAndMove(_lastFinalDirection);
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
        Vector3 origin = _rb.transform.position;
        Vector3 toTarget = _randomPointOnWaterSurface.Destination - origin;
        toTarget.y = 0f;
        Vector3 dirToTarget = toTarget.normalized;

        // Ligne de visée vers cible
        Gizmos.color = Color.red;
        Gizmos.DrawLine(origin, _randomPointOnWaterSurface.Destination);

        // Raycast réel vers cible
        Gizmos.color = _lastObstacleBetween ? Color.magenta : Color.cyan;
        Gizmos.DrawRay(origin, dirToTarget * _obstacleCheckDistance);

        // Directions d’évitement
        Vector3 leftAvoid = Quaternion.AngleAxis(-_avoidanceAngle, Vector3.up) * dirToTarget;
        Vector3 rightAvoid = Quaternion.AngleAxis(_avoidanceAngle, Vector3.up) * dirToTarget;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(origin, leftAvoid * 5f);
        Gizmos.DrawRay(origin, rightAvoid * 5f);

        // Direction finale choisie (vers cible ou avoidance)
        Gizmos.color = Color.green;
        Gizmos.DrawRay(origin, _lastFinalDirection * 5f);
    }
}
