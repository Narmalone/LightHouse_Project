using LightHouse.Game.World;
using UnityEngine;

public class WaterLensController : MonoBehaviour
{
    [Header("Refs")]
    public Material WaterLensMaterial;     // partagé avec RainController
    public RainController rainController;  // pour récupérer RainIntensity

    [Header("Player state")]
    public bool IsIndoors = false; // à set par ton système de trigger / volume

    [SerializeField] private float fadeSpeed = 0.5f; // unités par seconde

    // shader prop déjà défini dans RainController
    static readonly int _isRaining = Shader.PropertyToID("_isRaining");

    private void Awake()
    {
        GameZoneHandlerData.OnGameZoneChanged += GameZoneHandlerData_OnGameZoneChanged;
    }

    private void GameZoneHandlerData_OnGameZoneChanged(ZoneType obj)
    {
        IsIndoors = obj == ZoneType.Inside ? true : false;
    }

    private void OnDestroy()
    {
        GameZoneHandlerData.OnGameZoneChanged -= GameZoneHandlerData_OnGameZoneChanged;
    }

    void Update()
    {
        float targetIntensity = IsIndoors ? 0f : rainController.RainIntensity;
        float current = WaterLensMaterial.GetFloat(_isRaining);

        float newValue = Mathf.MoveTowards(current, targetIntensity, fadeSpeed * Time.deltaTime);
        WaterLensMaterial.SetFloat(_isRaining, newValue);
    }

}
