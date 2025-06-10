using UnityEngine;

public class BoatPathFollower : MonoBehaviour
{
    public Transform[] pathPoints; // Tes waypoints dans la scène
    public float moveSpeed = 5f;
    public float rotationSpeed = 2f;
    public float pointReachThreshold = 1f;

    public Rigidbody rb;

    private int currentPointIndex = 0;

    void FixedUpdate()
    {
        if (pathPoints.Length == 0 || rb == null) return;

        Vector3 targetPos = pathPoints[currentPointIndex].position;
        Vector3 direction = (targetPos - rb.position);
        Vector3 flatDirection = new Vector3(direction.x, 0f, direction.z); // empêche la rotation verticale

        // Si le bateau est proche du point, passer au suivant
        if (flatDirection.magnitude < pointReachThreshold)
        {
            currentPointIndex++;
            if (currentPointIndex >= pathPoints.Length)
                currentPointIndex = pathPoints.Length - 1; // Stop au dernier
            return;
        }

        // Appliquer le mouvement
        Vector3 moveForce = flatDirection.normalized * moveSpeed;
        rb.MovePosition(rb.position + moveForce * Time.fixedDeltaTime);

        // Tourner doucement sans basculer
        if (flatDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(flatDirection);
            Quaternion smoothRotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(smoothRotation);
        }
    }
}
