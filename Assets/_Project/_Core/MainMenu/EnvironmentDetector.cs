using UnityEngine;

public class EnvironmentDetector : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private LayerMask _skyBlockerMask;

    public bool IsPlayerOccluded()
    {
        Vector3 origin = _target.position + Vector3.up * 0.1f;
        return Physics.Raycast(origin, Vector3.up, 15.0f, _skyBlockerMask, QueryTriggerInteraction.Ignore);
    }
}
