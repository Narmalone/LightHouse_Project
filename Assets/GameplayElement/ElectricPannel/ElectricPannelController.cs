using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ElectricPannelController : MonoBehaviour
{
    [Header("Childs References")]
    [SerializeField] private MainDoorElectricalController _mainDoor;
    [SerializeField] private SwitchController[] switchs;
    [SerializeField] private NoFuelOnGenerator noFuelOnGenerator;

    [SerializeField] private Slider m_powerBar;

    [SerializeField] private ScenarioEvent m_fusibleEvent;

    [SerializeField] private float currentEnergyPower;
    [SerializeField, Tooltip("Not a percentage")] private float maxEnergyPower = 100f;

    public event Action<ElectricityZones> OnElectricityEnabled;
    public event Action<ElectricityZones> OnElectricityDisabled;

    public int numberOfEnabledSwitch =>  GetEnabledSwitchesNumber();

    private void Awake()
    {
        InitSwitches();
        m_fusibleEvent.eventAction += OnfusibleEventCalled;
    }

    public int GetEnabledSwitchesNumber()
    {
        int count = 0;
        foreach (var s in switchs)
        {
            if (s.IsEnabled)
            {
                count++;
            }
        }
            return count;
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
                }
                else
                {
                    AddOrRemovePower(s.CostPower);
                    OnElectricityDisabled?.Invoke(s.elecZone);
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

    public void OnFuelEmpty()
    {
        ShutdownAllSwitches();
        noFuelOnGenerator.gameObject.SetActive(true);
        SetEnableSwitches(false);
    }

    public void OnFuelFilledFromEmpty()
    {
        noFuelOnGenerator.gameObject.SetActive(false);
        SetEnableSwitches(true);
    }

    private void SetEnableSwitches(bool v)
    {
        foreach(var s in switchs)
        {
            s.Col.enabled = v;
        }
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

    private void OnfusibleEventCalled()
    {
        ShutdownAllSwitches();
    }
}
