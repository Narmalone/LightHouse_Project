using UnityEngine;

public class SimpleSteeringBehavior : IBoatSteeringBehavior
{
    public Vector3 GetSteeringDirection(Vector3 currentPosition, Vector3 targetPosition, Transform boatTransform)
    {
        Vector3 direction = (targetPosition - currentPosition);
        direction.y = 0f;
        return direction.normalized;
    }
}
