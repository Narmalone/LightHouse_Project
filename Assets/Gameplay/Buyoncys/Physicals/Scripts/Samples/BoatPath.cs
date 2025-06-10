using System;
using UnityEngine;

public class BoatPath : MonoBehaviour
{
    public Transform[] pathPoints;
    private int patrolIndex = 0;
    public float moveSpeed = 5f;
    public float rotationSpeed = 2f;
    
    public Rigidbody rb;

    private void FixedUpdate()
    {
        if (pathPoints.Length == 0 || rb == null) return;
        
        Transform target = pathPoints[patrolIndex];
        Vector3 dir = (target.position - this.transform.position).normalized;
        rb.linearVelocity = dir * moveSpeed;
        rb.angularVelocity = Vector3.Cross(this.transform.forward, dir) * rotationSpeed;
        
        if (Vector3.Distance(this.transform.position, target.position) < 0.1f)
        {
            patrolIndex++;
            if (patrolIndex >= pathPoints.Length)
            {
                patrolIndex = 0;
            }
        }
    }
}
