using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class WaterBoat : MonoBehaviour
{
    public WaterSurface targetSurface = null;

    public float speed = 5.0f; // boat speed
    public float turnSpeed = 2.0f; // boat turn speed
    public float waveAmplitude = 0.5f; // wave amplitude
    public float waveFrequency = 1.0f; // wave frequency

    // Internal search params
    WaterSearchParameters searchParameters = new WaterSearchParameters();
    WaterSearchResult searchResult = new WaterSearchResult();

    // Update is called once per frame
    void Update()
    {
        if (targetSurface != null)
        {
            // Update boat position
            searchParameters.startPositionWS = searchResult.candidateLocationWS;
            searchParameters.targetPositionWS = gameObject.transform.position;

            searchParameters.maxIterations = 8;

            if (targetSurface.ProjectPointOnWaterSurface(searchParameters, out searchResult))
            {
                gameObject.transform.position = searchResult.projectedPositionWS;

                // Update boat rotation (based on wave movement)
                float waveHeight = Mathf.Sin(Time.time * waveFrequency) * waveAmplitude;
                gameObject.transform.rotation = Quaternion.Euler(0, 0, waveHeight * 10);
            }

            // Update boat movement (forward and turning)
            float forwardMovement = speed * Time.deltaTime;
            float turnMovement = turnSpeed * Time.deltaTime;

            gameObject.transform.Translate(forwardMovement, 0, 0);
            gameObject.transform.Rotate(0, turnMovement, 0);
        }
    }
}