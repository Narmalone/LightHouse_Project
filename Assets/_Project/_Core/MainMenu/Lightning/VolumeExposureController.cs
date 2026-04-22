using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
public interface IExposureController
{
    void SetExposure(float value);
    void ResetExposure();
}
public class VolumeExposureController : MonoBehaviour, IExposureController
{
    [SerializeField] private Volume volume;

    private Exposure exposure;
    private float baseExposure;

    private void Awake()
    {
        if (volume.profile.TryGet(out exposure))
        {
            baseExposure = exposure.fixedExposure.value;
        }
    }

    public void SetExposure(float value)
    {
        if (exposure != null)
            exposure.fixedExposure.value = baseExposure + value;
    }

    public void ResetExposure()
    {
        if (exposure != null)
            exposure.fixedExposure.value = baseExposure;
    }
}