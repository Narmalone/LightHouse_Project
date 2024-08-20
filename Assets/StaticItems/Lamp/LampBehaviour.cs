using UnityEngine;

public class LampBehaviour : MonoBehaviour, IElectricityItem
{
    [SerializeField] private Light _light;

    public void OnElecDisable()
    {
        _light.enabled = false;
    }

    public void OnElecEnable()
    {
        _light.enabled = true;
    }

    private void Awake()
    {
        
    }
}
