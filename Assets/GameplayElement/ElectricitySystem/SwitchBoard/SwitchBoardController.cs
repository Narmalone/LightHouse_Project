using UnityEngine;
using UnityEngine.UI;

public class SwitchBoardController : MonoBehaviour
{
    #region SERIALIZED REFERENCES
    [Header("Childs References")]
    [SerializeField] private MainDoorElectricalController _mainDoor;
    [SerializeField] private SwitchController[] switchs;
    [SerializeField] private NoFuelOnGenerator noFuelOnGenerator;

    [Header("UI")]
    [SerializeField] private Slider m_powerBar;

    [Header("GAMEPLAY")]
    [SerializeField] private float currentEnergyPower;
    [SerializeField, Tooltip("Not a percentage")] private float maxEnergyPower = 100f;

    [Header("--- EVENTS ---")]
    [Header("RAISE")]
    [SerializeField] private CustomEvent_ElectricZone _onSwitchBoardEnabled;
    [SerializeField] private CustomEvent_ElectricZone _onSwitchBoardDisabled;
    [SerializeField] private CustomEvent _onAllSwitchesShutdown;

    [Header("LISTENERS")]
    [Header("Scenario")]
    [SerializeField] private ScenarioEvent m_fusibleEvent;

    #endregion

    #region PUBLIC PROPERTIES
    public int numberOfEnabledSwitch =>  GetEnabledSwitchesNumber();
    #endregion

    #region UNITY MONO CALLBACKS

    private void Awake()
    {
        InitSwitches();
        m_fusibleEvent.EventAction += OnfusibleEventCalled;
    }

    private void Start()
    {
        currentEnergyPower = maxEnergyPower;
        UpdatePowerUI();
    }

    private void OnDestroy()
    {
        m_fusibleEvent.EventAction -= OnfusibleEventCalled;
    }

    #endregion

    #region INITS FUNC & GET

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
                    _onSwitchBoardEnabled?.Raise(s.elecZone);
                }
                else
                {
                    AddOrRemovePower(s.CostPower);
                    _onSwitchBoardDisabled?.Raise(s.elecZone);
                }
            };
        }
    }

    #endregion

    #region UI FUNCS

    private void UpdatePowerUI()
    {
        m_powerBar.value = (currentEnergyPower / maxEnergyPower);
    }

    #endregion

    #region POWER FUNCS

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

    #endregion

    #region PUBLICS FUNCS FROM OTHER SCRIPTS / DELEGATES
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

    #endregion

    #region SWITCHES FUNCS

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
        _onAllSwitchesShutdown?.Raise();
    }

    #endregion

    #region DELEGATE LISTENERS
    private void OnfusibleEventCalled()
    {
        ShutdownAllSwitches();
    }

    #endregion
}
