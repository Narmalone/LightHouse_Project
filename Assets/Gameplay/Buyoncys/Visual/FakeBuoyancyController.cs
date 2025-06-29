using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class PrecisionFakeBuoyancyController : MonoBehaviour
{
    [SerializeField] private WaterSurface water;
    [SerializeField] private Transform[] floaters; // 0–12 = gauche, 13–25 = droite (arrière → avant)

    [Header("Water Search Settings")]
    [SerializeField] private float error = 0.01f;
    [SerializeField] private int maxIterations = 10;
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
        if (floaters == null || floaters.Length != 26 || water == null)
        {
            Debug.LogWarning("Ce script attend exactement 26 floaters (13 gauche, 13 droite).");
            return;
        }

        Vector3 avgPos = Vector3.zero;
        float leftSum = 0f;
        float rightSum = 0f;
        float backSum = 0f;
        float frontSum = 0f;

        int half = floaters.Length / 2;

        for (int i = 0; i < floaters.Length; i++)
        {
            Vector3 worldPos = floaters[i].position;

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
                avgPos += (Vector3)searchResult.projectedPositionWS;
            }
            else
            {
                avgPos += worldPos;
            }

            // Somme par côté
            if (i < half) leftSum += projectedY;
            else rightSum += projectedY;

            // Arrière / Avant (basé sur index, arrière d'abord)
            if (i % 13 < 6) backSum += projectedY;   // index 0–5 et 13–18 → arrière
            else frontSum += projectedY;            // index 6–12 et 19–25 → avant
        }

        avgPos /= floaters.Length;

        // Position verticale lissée
        Vector3 targetPos = transform.position;
        targetPos.y = avgPos.y;
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * positionLerpSpeed);

        // Calcul du pitch (avant/arrière) et roll (gauche/droite)
        float pitch = (frontSum - backSum) / floaters.Length;
        float roll = (leftSum - rightSum) / half;

        Quaternion targetRot = Quaternion.Euler(
            pitch * rotationFactor,
            transform.eulerAngles.y,
            -roll * rotationFactor
        );

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationLerpSpeed);
    }
}
