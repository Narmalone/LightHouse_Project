using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class BuoyancyPoseFromProbes : MonoBehaviour
{
    [Header("Water Settings")]
    [SerializeField] private WaterSurface waterSurface;
    [SerializeField] private Transform[] probes;

    [Header("Water Search Parameters")]
    [SerializeField] private float searchError = 0.01f;
    [SerializeField, Range(1, 100)] private int maxIterations = 10;
    [SerializeField] private bool includeDeformation = true;
    [SerializeField] private bool excludeSimulation = false;

    [Header("Smoothing")]
    [SerializeField] private float positionLerpSpeed = 5f;
    [SerializeField] private float rotationLerpSpeed = 2f;
    [SerializeField] private float rotationAmplitude = 30f;

    private WaterSearchParameters searchParams = new WaterSearchParameters();
    private WaterSearchResult searchResult = new WaterSearchResult();

    void Update()
    {
        if (probes == null || probes.Length < 4 || waterSurface == null) return;

        Vector3 avgPos = Vector3.zero;
        float[] heights = new float[probes.Length];
        Vector3[] positions = new Vector3[probes.Length];

        for (int i = 0; i < probes.Length; i++)
        {
            Vector3 probePos = probes[i].position;
            positions[i] = probePos;

            searchParams.startPositionWS = probePos;
            searchParams.targetPositionWS = probePos;
            searchParams.error = searchError;
            searchParams.maxIterations = maxIterations;
            searchParams.includeDeformation = includeDeformation;
            searchParams.excludeSimulation = excludeSimulation;

            if (waterSurface.ProjectPointOnWaterSurface(searchParams, out searchResult))
            {
                heights[i] = searchResult.projectedPositionWS.y;
                avgPos += (Vector3)searchResult.projectedPositionWS;
            }
            else
            {
                heights[i] = probePos.y;
                avgPos += probePos;
            }
        }

        avgPos /= probes.Length;

        // Position
        Vector3 targetPosition = transform.position;
        targetPosition.y = avgPos.y;
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * positionLerpSpeed);

        // Rotation (pitch/roll)
        // ➤ Assumes first 4 probes are in this order:
        // 0: FrontLeft, 1: FrontRight, 2: BackLeft, 3: BackRight
        float pitch = ((heights[0] + heights[1]) - (heights[2] + heights[3])) * 0.5f;
        float roll = ((heights[1] + heights[3]) - (heights[0] + heights[2])) * 0.5f;

        Quaternion targetRotation = Quaternion.Euler(
            pitch * rotationAmplitude,
            transform.eulerAngles.y,
            -roll * rotationAmplitude
        );

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationLerpSpeed);
    }
}
