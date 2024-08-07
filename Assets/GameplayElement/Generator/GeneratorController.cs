using System;
using UnityEngine;
using static UnityEditor.Progress;

public class GeneratorController : MonoBehaviour
{
    [SerializeField] private ScenarioEvent m_shutdownEvent;

    [Header("LIFE 3D BAR")]
    [SerializeField] private Transform m_targetLife;
    private Vector3 startPosition;
    private Vector3 endPosition; 
    private Vector3 startScale; 
    public float endScaleY = 0f;

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
                m_updateFuel = false;
                OnGeneratorsFuelEmpty();
            }
        }
    }
    [SerializeField, ConsoleVariable("MaxFuel")] private float m_maxFuelValue = 100f;
    [SerializeField, Range(0, 8), ConsoleVariable("GenSpeedSec")] private float m_speedDecreasePerSecond = 0.1f;
    [SerializeField, Range(0, 10), ConsoleVariable("GenSpeedMul")] private float m_speedDecreaseMultiplier = 1f;

    [SerializeField, ConsoleVariable("CanUpdateFuel")] private bool m_updateFuel = false;

    [SerializeField] private JerricanEssence currentJerricanSelected = null;

    private void Awake()
    {
        m_fuelValue = m_maxFuelValue;
        startPosition = m_targetLife.position;
        startScale = m_targetLife.localScale;
        endPosition = startPosition + (Vector3.down * 2);
        m_shutdownEvent.eventAction += OnEventCalled;

        m_triggerFuel.OnEntered += M_triggerFuel_OnEntered;
        m_triggerFuel.OnExited += M_triggerFuel_OnExited;
    }

    private void PlayerInventory_OnCurrentItemSelectedChanged(ItemBase arg1, ItemBase arg2)
    {
        if (arg2 as JerricanEssence)
        {
            if(currentJerricanSelected != null)
            {
                currentJerricanSelected.isUsable = false;
                currentJerricanSelected.OnJericanUse -= Jerrican_OnJericanUse;
            }
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
        AddFuelValue(obj);
        currentJerricanSelected.OnJericanUse -= Jerrican_OnJericanUse;
        PlayerManager.Instance._inventory.RemoveItemFromInventory(currentJerricanSelected);
        currentJerricanSelected = null;
    }

    private void OnDestroy()
    {
        m_shutdownEvent.eventAction -= OnEventCalled;

        m_triggerFuel.OnEntered -= M_triggerFuel_OnEntered;
        m_triggerFuel.OnExited -= M_triggerFuel_OnExited;
    }

    private void OnEventCalled()
    {
        //SetFuelValue(0f);
    }

    private void Update()
    {
        if(m_updateFuel)
            UpdateFuel();
    }

    private void OnFuelValueChange(float value)
    {
        float t = 1 - (m_fuelValue / m_maxFuelValue);
        m_targetLife.position = Vector3.Lerp(startPosition, endPosition, t);
        m_targetLife.localScale = new Vector3(startScale.x, Mathf.Lerp(startScale.y, endScaleY, t), startScale.z);
    }

    private void OnGeneratorsFuelEmpty()
    {
        //jouer anims, sons...
    }

    private void UpdateFuel()
    {
        FuelValue -= m_speedDecreasePerSecond * m_speedDecreaseMultiplier * Time.deltaTime;
    }

    private void SetFuelValue(float _fuelValue)
    {
        FuelValue = _fuelValue;
    }

    private void AddFuelValue(float _fuelValue)
    {
        FuelValue += _fuelValue;
    }
    
    private void RemoveFuelValue(float _fuelValue)
    {
        FuelValue -= _fuelValue;
    }
}
