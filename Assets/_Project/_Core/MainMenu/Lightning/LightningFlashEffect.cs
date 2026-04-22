using System.Collections;
using UnityEngine;

public class LightningFlashEffect : MonoBehaviour
{
    [SerializeField] private Light flashLight;
    [SerializeField] private MonoBehaviour exposureController; // injecté

    private IExposureController exposure;

    public float intensity = 50000f;
    public float exposureIntensity = 2.0f;
    public float duration = 0.3f;

    private void Awake()
    {
        exposure = exposureController as IExposureController;

        if (exposure == null)
            Debug.LogError("ExposureController must implement IExposureController");
    }

    public void PlayFlash(Vector3 position)
    {
        StartCoroutine(FlashRoutine(position));
    }

    private IEnumerator FlashRoutine(Vector3 pos)
    {
        flashLight.transform.position = pos;
        flashLight.enabled = true;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Sin((t / duration) * Mathf.PI);

            flashLight.intensity = intensity * k;

            if (exposure != null)
                exposure.SetExposure(-k * exposureIntensity);

            yield return null;
        }

        flashLight.enabled = false;

        exposure?.ResetExposure();
    }
}