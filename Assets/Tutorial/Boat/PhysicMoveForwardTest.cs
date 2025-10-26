using KinematicCharacterController;
using UnityEngine;

public class PhysicMoveForwardTest : MonoBehaviour
{
    public Rigidbody body;
    public Vector3 force;
    public float multiplier = 1f;
    public PhysicsMover mover;
    private void FixedUpdate()
    {
        body.AddForce(mover.GetState().Velocity * multiplier, ForceMode.Force);
    }
}
