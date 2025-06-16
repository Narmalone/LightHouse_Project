using UnityEngine;

public class ObstacleAvoidanceSteering : IBoatSteeringBehavior
{
    private readonly LayerMask _obstacleMask;
    private readonly float _rayDistance;
    private readonly float _avoidStrength;

    public ObstacleAvoidanceSteering(LayerMask obstacleMask, float rayDistance = 15f, float avoidStrength = 1f)
    {
        _obstacleMask = obstacleMask;
        _rayDistance = rayDistance;
        _avoidStrength = avoidStrength;
    }

    public Vector3 GetSteeringDirection(Vector3 currentPosition, Vector3 targetPosition, Transform boatTransform)
    {
        Vector3 desiredDir = (targetPosition - currentPosition).normalized;
        desiredDir.y = 0f;

        Vector3 avoidance = Vector3.zero;

        RaycastHit hit;
        if (Physics.Raycast(currentPosition, boatTransform.forward, out hit, _rayDistance, _obstacleMask))
        {
            avoidance += hit.normal * _avoidStrength;
        }

        Vector3 finalDir = (desiredDir + avoidance).normalized;
        return finalDir;
    }
}
