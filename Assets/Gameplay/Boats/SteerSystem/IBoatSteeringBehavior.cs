using UnityEngine;

public interface IBoatSteeringBehavior
{
    Vector3 GetSteeringDirection(Vector3 currentPosition, Vector3 targetPosition, Transform boatTransform);
}
