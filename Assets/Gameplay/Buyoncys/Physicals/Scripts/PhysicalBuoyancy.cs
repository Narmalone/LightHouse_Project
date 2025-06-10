using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class PhysicalBuoyancy : MonoBehaviour
{
    public Rigidbody rb;
    public float depthBeforeSubmersion = 0.5f;
    public float displacementAmount = 3f;
    public float waterDrag = 0.99f;
    public float waterAngularDrag = 0.5f;
    public WaterSurface water;

    private WaterSearchParameters waterSearchParameters;
    private WaterSearchResult waterSearchResult;

    private void FixedUpdate()
    {
        // Gravit�
        rb.AddForceAtPosition(Physics.gravity / 4f, transform.position, ForceMode.Acceleration);

        // Hauteur de l'eau au point actuel
        waterSearchParameters.startPositionWS = transform.position;
        water.ProjectPointOnWaterSurface(waterSearchParameters, out waterSearchResult);
        float waterHeight = waterSearchResult.projectedPositionWS.y;

        if (transform.position.y < waterHeight)
        {
            float depth = waterHeight - transform.position.y;
            float displacementMultiplier = Mathf.Clamp01(depth / depthBeforeSubmersion) * displacementAmount;

            // Pouss�e vers le haut (Archim�de)
            Vector3 uplift = Vector3.up * Mathf.Abs(Physics.gravity.y) * displacementMultiplier;
            rb.AddForceAtPosition(uplift, transform.position, ForceMode.Acceleration);

            // Amortissement (drag)
            rb.AddForce(-rb.linearVelocity * waterDrag * displacementMultiplier * Time.fixedDeltaTime, ForceMode.VelocityChange);
            rb.AddTorque(-rb.angularVelocity * waterAngularDrag * displacementMultiplier * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }
    }
}
