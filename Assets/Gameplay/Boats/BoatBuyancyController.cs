using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class BoatBuoyancyController : MonoBehaviour
{
    [SerializeField] private Transform[] buoyancyPoints;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private WaterSurface water;

    [Header("Buoyancy Settings")]
    [SerializeField] private float depthBeforeSubmersion = 1f;
    [SerializeField] private float displacementAmount = 1f;
    [SerializeField] private float maxUpliftForce = 10f;

    [Header("Drag Settings")]
    [SerializeField] private float waterDrag = 1f;
    [SerializeField] private float waterAngularDrag = 1f;

    private WaterSearchParameters waterParams;
    private WaterSearchResult waterResult;

    private void FixedUpdate()
    {
        float totalUplift = 0f;

        foreach (Transform point in buoyancyPoints)
        {
            waterParams.startPositionWS = point.position;
            water.ProjectPointOnWaterSurface(waterParams, out waterResult);

            float waterHeight = waterResult.projectedPositionWS.y;
            float depth = waterHeight - point.position.y;

            if (depth > 0f)
            {
                float displacementMultiplier = Mathf.Clamp01(depth / depthBeforeSubmersion) * displacementAmount;
                float uplift = Mathf.Clamp(
                    displacementMultiplier * Mathf.Abs(Physics.gravity.y) * rb.mass / buoyancyPoints.Length,
                    0,
                    maxUpliftForce
                );

                rb.AddForceAtPosition(Vector3.up * uplift, point.position, ForceMode.Force);
                totalUplift += uplift;
                Debug.DrawLine(point.position, point.position + Vector3.up * uplift * 0.1f, Color.green);
            }
        }

        // Appliquer le drag UNE SEULE FOIS
        rb.AddForce(-rb.linearVelocity * waterDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);
        rb.AddTorque(-rb.angularVelocity * waterAngularDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);
    }

}
