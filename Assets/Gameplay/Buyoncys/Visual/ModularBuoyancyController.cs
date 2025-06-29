using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class ModularBuoyancyController : MonoBehaviour
{
    [Header("Floaters")]
    [Tooltip("Transform points sous la coque, à répartir autour du centre du bateau.")]
    [SerializeField] private Transform[] floaters;

    [Header("Water Surface")]
    [SerializeField] private WaterSurface water;
    [SerializeField] private float error = 0.01f;
    [SerializeField, Range(1, 100)] private int maxIterations = 10;
    [SerializeField] private bool includeDeformation = true;
    [SerializeField] private bool excludeSimulation = false;

    [Header("Smoothing")]
    [SerializeField] private float positionLerpSpeed = 5f;
    [SerializeField] private float rotationLerpSpeed = 2f;
    [SerializeField] private float rotationFactor = 30f;

    private WaterSearchParameters searchParams = new WaterSearchParameters();
    private WaterSearchResult searchResult = new WaterSearchResult();

    void Update()
    {
        if (floaters == null || floaters.Length < 4 || water == null)
            return;

        Vector3 center = transform.position;
        Vector3 avgProjected = Vector3.zero;
        float leftY = 0f, rightY = 0f, frontY = 0f, backY = 0f;
        int leftCount = 0, rightCount = 0, frontCount = 0, backCount = 0;

        for (int i = 0; i < floaters.Length; i++)
        {
            Vector3 worldPos = floaters[i].position;

            // Setup water params
            searchParams.startPositionWS = worldPos;
            searchParams.targetPositionWS = worldPos;
            searchParams.error = error;
            searchParams.maxIterations = maxIterations;
            searchParams.includeDeformation = includeDeformation;
            searchParams.excludeSimulation = excludeSimulation;

            float projectedY = worldPos.y;
            if (water.ProjectPointOnWaterSurface(searchParams, out searchResult))
            {
                projectedY = searchResult.projectedPositionWS.y;
                avgProjected += (Vector3)searchResult.projectedPositionWS;

                // Détermination gauche/droite
                if (worldPos.x < center.x) { leftY += projectedY; leftCount++; }
                else { rightY += projectedY; rightCount++; }

                // Détermination avant/arrière
                if (worldPos.z > center.z) { frontY += projectedY; frontCount++; }
                else { backY += projectedY; backCount++; }
            }
        }

        avgProjected /= floaters.Length;

        // Position verticale
        Vector3 newPos = transform.position;
        newPos.y = avgProjected.y;
        transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * positionLerpSpeed);

        // Roll & Pitch
        float avgLeft = leftCount > 0 ? leftY / leftCount : 0f;
        float avgRight = rightCount > 0 ? rightY / rightCount : 0f;
        float avgFront = frontCount > 0 ? frontY / frontCount : 0f;
        float avgBack = backCount > 0 ? backY / backCount : 0f;

        float roll = (avgLeft - avgRight) * rotationFactor;
        float pitch = (avgFront - avgBack) * rotationFactor;

        Quaternion targetRot = Quaternion.Euler(pitch, transform.eulerAngles.y, -roll);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationLerpSpeed);
    }
}
