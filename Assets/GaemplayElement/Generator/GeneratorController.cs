using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorController : MonoBehaviour
{
    [SerializeField] private ScenarioEvent m_generatorEvent;
    [SerializeField] private Transform m_targetLife;
    [SerializeField] private float m_fuelValue = 100f;
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
    [SerializeField] private float m_maxFuelValue = 100f;
    [SerializeField, Range(0, 8)] private float m_speedDecreasePerSecond = 0.1f;
    [SerializeField, Range(0, 10)] private float m_speedDecreaseMultiplier = 1f;

    private bool m_updateFuel = false;

    private void Awake()
    {
        m_fuelValue = m_maxFuelValue;
    }

    private void Update()
    {
        if(m_updateFuel)
            UpdateFuel();
    }

    private void OnFuelValueChange(float value)
    {
        Debug.Log(value);
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
