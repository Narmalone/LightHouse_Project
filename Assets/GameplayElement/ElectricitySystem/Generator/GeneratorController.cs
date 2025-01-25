using System;
using UnityEngine;

public class GeneratorController : MonoBehaviour
{
    [Header("EVENTS")]
    [SerializeField] private ScenarioEvent m_shutdownEvent;
    [SerializeField] private CustomEvent OnGeneratorEnabled;
    [SerializeField] private CustomEvent OnGeneratorDisabled;
    [SerializeField] private CustomEvent OnGeneratorFuelEmpty;
    [SerializeField] private CustomEvent OnGeneratorFuelFilledFromEmpty;

    [Header("CONTROLLERS")]
    [SerializeField] private HandleGenerator handleController;
    [SerializeField] private BoutonGenerator btnController;
    [SerializeField] private FuelLid fuelLid;

    [Header("GENERATOR")]
    [SerializeField] private float m_fuelValue = 100f;
    [SerializeField] private TriggerEvent m_triggerFuel;

    [ConsoleVariable("Fuel")]
    public float FuelValue
    {
        get { return m_fuelValue; }
        set
        {
            if (value != m_fuelValue)
            {
                OnFuelValueChange(value);
            }
            m_fuelValue = value;
            if (m_fuelValue > m_maxFuelValue) m_fuelValue = m_maxFuelValue;
            else if (m_fuelValue < 0f)
            {
                m_fuelValue = 0f;
                IsOn = false;
                OnGeneratorsFuelEmpty();
            }
        }
    }
    [SerializeField, ConsoleVariable("MaxFuel")] private float m_maxFuelValue = 100f;
    [SerializeField, Range(0, 8), ConsoleVariable("GenSpeedSec")] private float m_speedDecreasePerSecond = 0.1f;
    [SerializeField, Range(0, 10), ConsoleVariable("GenSpeedMul")] private float m_speedDecreaseMultiplier = 1f;

    [SerializeField, ConsoleVariable("CanUpdateFuel")] public bool IsOn = false;
    private JerricanEssence currentJerricanSelected = null;

    private void Awake()
    {
        m_fuelValue = m_maxFuelValue;
        //m_shutdownEvent.EventAction += OnEventCalled;

        m_triggerFuel.OnEntered += M_triggerFuel_OnEntered;
        m_triggerFuel.OnExited += M_triggerFuel_OnExited;


        btnController.OnChanged += SecondaryBtnController_OnChanged;
        handleController.OnChanged += HandleController_OnChanged;
        fuelLid.OnChanged += FuelLid_OnChanged;

        StartInit();
    }

    private void StartInit()
    {
        m_triggerFuel.gameObject.SetActive(fuelLid.IsOpen);

    }

    private void FuelLid_OnChanged(bool obj)
    {
        if (obj)
        {
            m_triggerFuel.gameObject.SetActive(true);
        }
        else
        {
            m_triggerFuel.gameObject.SetActive(false);

            if(currentJerricanSelected != null)
            {
                currentJerricanSelected.OnJericanUse -= Jerrican_OnJericanUse;
                currentJerricanSelected.isUsable = false;
                currentJerricanSelected = null;
            }
            PlayerInventory.OnCurrentItemSelectedChanged -= PlayerInventory_OnCurrentItemSelectedChanged;
        }
    }

    private void SetGeneratorState(bool value)
    {
        if (value)
        {
            btnController.IsEnabled = true;
            handleController.IsEnabled = true;
        }
        else
        {
            btnController.IsEnabled = false;
            handleController.IsEnabled = false;
        }
    }

    private void HandleController_OnChanged(bool obj)
    {
        bool condition = CheckCondition();
        if (condition)
        {
            OnGeneratorEnabled?.Raise();
        }
        else if (condition != IsOn && obj == false)
        {
            OnGeneratorDisabled?.Raise();
        }
        IsOn = condition;
    }

    private void SecondaryBtnController_OnChanged(bool obj)
    {
        bool condition = CheckCondition();
        if (condition)
        {
            OnGeneratorEnabled?.Raise();
        }
        else if (condition != IsOn && obj == false)
        {
            OnGeneratorDisabled?.Raise();
        }
        IsOn = condition;
    }

    private bool CheckCondition()
    {
        return btnController.IsEnabled && handleController.IsEnabled;
    }

    private void PlayerInventory_OnCurrentItemSelectedChanged(ItemBase arg1, ItemBase arg2)
    {
        if (currentJerricanSelected != null)
        {
            currentJerricanSelected.isUsable = false;
            currentJerricanSelected.OnJericanUse -= Jerrican_OnJericanUse;
        }

        if (arg2 as JerricanEssence)
        {
            currentJerricanSelected = arg2 as JerricanEssence;
            currentJerricanSelected.isUsable = true;
            currentJerricanSelected.OnJericanUse += Jerrican_OnJericanUse;
        }
    }

    private void M_triggerFuel_OnExited(GameObject obj)
    {
        if(currentJerricanSelected != null)
        {
            currentJerricanSelected.OnJericanUse -= Jerrican_OnJericanUse;
            currentJerricanSelected.isUsable = false;
            currentJerricanSelected = null;
        }
        PlayerInventory.OnCurrentItemSelectedChanged -= PlayerInventory_OnCurrentItemSelectedChanged;
    }

    private void M_triggerFuel_OnEntered(GameObject obj)
    {
        var item = PlayerManager.Instance._inventory.CurrentItemSelected;
        PlayerInventory.OnCurrentItemSelectedChanged += PlayerInventory_OnCurrentItemSelectedChanged;
        if (item as JerricanEssence)
        {
            item.isUsable = true;
            currentJerricanSelected = item as JerricanEssence;
            currentJerricanSelected.OnJericanUse += Jerrican_OnJericanUse;
        }
    }

    private void Jerrican_OnJericanUse(float obj)
    {
        AddPercentFuelValue(obj);
        if (obj > 0f && IsOn == false)
        {
            IsOn = true;
            OnGeneratorFuelFilledFromEmpty?.Raise();
        }
        currentJerricanSelected.OnJericanUse -= Jerrican_OnJericanUse;
        PlayerManager.Instance._inventory.RemoveItemFromInventory(currentJerricanSelected);
        currentJerricanSelected = null;
    }

    private void OnDestroy()
    {
        //m_shutdownEvent.EventAction -= OnEventCalled;

        m_triggerFuel.OnEntered -= M_triggerFuel_OnEntered;
        m_triggerFuel.OnExited -= M_triggerFuel_OnExited;

        btnController.OnChanged -= SecondaryBtnController_OnChanged;
        handleController.OnChanged -= HandleController_OnChanged;
    }

    private void OnEventCalled()
    {
        
    }

    private void Update()
    {
        if(IsOn)
            UpdateFuel();
    }

    private void OnFuelValueChange(float value)
    {
        
    }

    private void OnGeneratorsFuelEmpty()
    {
        //jouer anims, sons...
        handleController.IsEnabled = false;
        btnController.IsEnabled = false;
        OnGeneratorFuelEmpty?.Raise();
    }

    private void UpdateFuel()
    {
        FuelValue -= m_speedDecreasePerSecond * m_speedDecreaseMultiplier * Time.deltaTime;
    }

    private void AddPercentFuelValue(float _percentFuel)
    {
        FuelValue = Mathf.Clamp(FuelValue + (m_maxFuelValue * (_percentFuel / 100f)), 0f, m_maxFuelValue);
    }
}
