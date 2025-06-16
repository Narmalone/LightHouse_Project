using UnityEngine;

public class SmartAvoidanceSteering : IBoatSteeringBehavior
{
    private readonly LayerMask _obstacleMask;
    private readonly float _rayDistance;
    private readonly float _sideCheckAngle;
    private readonly float _sideOffsetAngle;

    private enum AvoidDirection { None, Left, Right }
    private AvoidDirection _currentAvoidance = AvoidDirection.None;

    private float _clearPathTimer = 0f;
    private const float _requiredClearTime = 1.0f;

    public SmartAvoidanceSteering(
        LayerMask obstacleMask,
        float rayDistance = 20f,
        float sideCheckAngle = 30f,
        float sideOffsetAngle = 60f)
    {
        _obstacleMask = obstacleMask;
        _rayDistance = rayDistance;
        _sideCheckAngle = sideCheckAngle;
        _sideOffsetAngle = sideOffsetAngle;
    }

    public Vector3 GetSteeringDirection(Vector3 currentPosition, Vector3 targetPosition, Transform boatTransform)
    {
        Vector3 forward = boatTransform.forward;
        Vector3 toTarget = (targetPosition - currentPosition).normalized;
        toTarget.y = 0;

        bool forwardBlocked = Physics.Raycast(currentPosition, forward, _rayDistance, _obstacleMask);

        // Évitement actif
        if (_currentAvoidance != AvoidDirection.None)
        {
            if (!forwardBlocked)
            {
                _clearPathTimer += Time.fixedDeltaTime;

                if (_clearPathTimer >= _requiredClearTime)
                {
                    _currentAvoidance = AvoidDirection.None;
                    _clearPathTimer = 0f;
                    return toTarget;
                }
            }
            else
            {
                _clearPathTimer = 0f;
            }

            float angle = _currentAvoidance == AvoidDirection.Left ? -_sideOffsetAngle : _sideOffsetAngle;
            return Quaternion.AngleAxis(angle, Vector3.up) * forward;
        }

        // Pas de blocage, cap direct
        if (!forwardBlocked)
        {
            return toTarget;
        }

        // Blocage détecté → choix d’un côté
        Vector3 leftDir = Quaternion.AngleAxis(-_sideCheckAngle, Vector3.up) * forward;
        Vector3 rightDir = Quaternion.AngleAxis(_sideCheckAngle, Vector3.up) * forward;

        bool leftClear = !Physics.Raycast(currentPosition, leftDir, _rayDistance, _obstacleMask);
        bool rightClear = !Physics.Raycast(currentPosition, rightDir, _rayDistance, _obstacleMask);

        if (leftClear && !rightClear)
        {
            _currentAvoidance = AvoidDirection.Left;
            return Quaternion.AngleAxis(-_sideOffsetAngle, Vector3.up) * forward;
        }
        else if (rightClear && !leftClear)
        {
            _currentAvoidance = AvoidDirection.Right;
            return Quaternion.AngleAxis(_sideOffsetAngle, Vector3.up) * forward;
        }
        else if (leftClear && rightClear)
        {
            _currentAvoidance = Vector3.Dot(leftDir, toTarget) > Vector3.Dot(rightDir, toTarget)
                ? AvoidDirection.Left
                : AvoidDirection.Right;

            float chosenAngle = _currentAvoidance == AvoidDirection.Left ? -_sideOffsetAngle : _sideOffsetAngle;
            return Quaternion.AngleAxis(chosenAngle, Vector3.up) * forward;
        }

        // Rien n'est libre → tourner lentement
        return Quaternion.AngleAxis(45f, Vector3.up) * forward;
    }
}
