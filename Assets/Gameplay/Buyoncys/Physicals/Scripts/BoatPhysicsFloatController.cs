using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class BoatPhysicsFloatController : MonoBehaviour
{
    [Header("Water")]
    public WaterSurface water;

    [Header("Floaters")]
    public Transform[] floaters;

    [Header("Physics")]
    public Rigidbody rb;
    public float depthBeforeSubmersion = 0.5f;
    public float displacementAmount = 3f;
    public float waterDrag = 1f;
    public float waterAngularDrag = 1f;

    [Header("Search Settings")]
    public float searchError = 0.01f;
    [Range(1, 200)] public int maxIterations = 10;
    public bool includeDeformation = true;
    public bool excludeSimulation = false;
    public bool outputNormal = false;

    private void FixedUpdate()
    {
        if (rb == null || water == null || floaters == null || floaters.Length == 0) return;

        // Répartir la gravité (important !)
        foreach (Transform floater in floaters)
        {
            rb.AddForceAtPosition(Physics.gravity / floaters.Length, floater.position, ForceMode.Acceleration);

            WaterSearchParameters param = new WaterSearchParameters
            {
                startPositionWS = floater.position,
                targetPositionWS = floater.position,
                error = searchError,
                maxIterations = maxIterations,
                includeDeformation = includeDeformation,
                excludeSimulation = excludeSimulation,
                outputNormal = outputNormal
            };

            if (water.ProjectPointOnWaterSurface(param, out WaterSearchResult result))
            {
                float waterHeight = result.projectedPositionWS.y;
                float depth = waterHeight - floater.position.y;

                if (depth > 0f)
                {
                    float displacementMultiplier = Mathf.Clamp01(depth / depthBeforeSubmersion) * displacementAmount;
                    Vector3 uplift = Vector3.up * Mathf.Abs(Physics.gravity.y) * displacementMultiplier;

                    rb.AddForceAtPosition(uplift, floater.position, ForceMode.Acceleration);

                    rb.AddForce(-rb.linearVelocity * waterDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);
                    rb.AddTorque(-rb.angularVelocity * waterAngularDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);

                    Debug.DrawLine(floater.position, floater.position + uplift * 0.1f, Color.cyan);
                }
            }
        }
    }
}
