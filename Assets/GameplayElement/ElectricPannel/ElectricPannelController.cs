using System;
using UnityEngine;
using UnityEngine.UI;

public class ElectricPannelController : MonoBehaviour
{
    [Header("Childs References")]
    [SerializeField] private ItemBaseAnim _mainDoor;
    [SerializeField] private SwitchController[] switchs;

    [SerializeField] private Slider m_powerBar;

    [SerializeField] private ScenarioEvent m_fusibleEvent;

    [SerializeField] private float currentEnergyPower;
    [SerializeField, Tooltip("Not a percentage")] private float maxEnergyPower = 100f;

    public static event Action<ElectricityZones> OnElectricityEnabled;
    public static event Action<ElectricityZones> OnElectricityDisabled;

    private void Awake()
    {
        InitSwitches();
        m_fusibleEvent.eventAction += OnfusibleEventCalled;
    }

    private void InitSwitches()
    {
        foreach (var s in switchs)
        {
            s.OnUse += () =>
            {
                if (s.IsEnabled)
                {
                    AddOrRemovePower(-s.CostPower);
                    OnElectricityEnabled?.Invoke(s.elecZone);
                    Debug.Log("Enable all items in " + s.elecZone);
                }
                else
                {
                    AddOrRemovePower(s.CostPower);
                    OnElectricityDisabled?.Invoke(s.elecZone);
                    Debug.Log("Disable all items in " + s.elecZone);
                }
            };
        }
    }

    private void Start()
    {
        currentEnergyPower = maxEnergyPower;
        UpdatePowerUI();
    }

    private void OnDestroy()
    {
        m_fusibleEvent.eventAction -= OnfusibleEventCalled;
    }

    private void UpdatePowerUI()
    {
        m_powerBar.value = (currentEnergyPower / maxEnergyPower);
    }

    private void AddOrRemovePower(float power)
    {
        currentEnergyPower += power;
        if (currentEnergyPower / maxEnergyPower < 0f)
        {
            currentEnergyPower = 0f;
            ShutdownAllSwitches();
        }
        else if (currentEnergyPower >= maxEnergyPower) currentEnergyPower = maxEnergyPower;
        UpdatePowerUI();
    }

    private void ShutdownAllSwitches()
    {
        foreach(var s in switchs)
        {
            if (s.IsEnabled)
            {
                s.IsEnabled = false;
                AddOrRemovePower(s.CostPower);
            }
        }
    }

    //Remove un fusible au hasard et désactiver l'électricitée, le joueur devras en racheter un et le remplacer
    private void OnfusibleEventCalled()
    {
        ShutdownAllSwitches();
    }
}
