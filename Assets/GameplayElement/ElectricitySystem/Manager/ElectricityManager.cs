using System.Collections.Generic;
using UnityEngine;

public class ElectricityManager : Singleton<ElectricityManager>
{
    //ENlever le syst ou quand y'a plus de fuel ça shutdown et plus tot ça ne fais juste plus passer le courant
    //Faire en sorte que lorsque la mise à jour des zones actives soit bonne
    //trouver un moyen afin d'appeler de manière optimale l'event désactiver / activer objet niv électrique
    #region SERIALIZED VARIABLES

    [Header("CONTROLLERS REFERENCES")]
    [SerializeField] private SwitchBoardController _switchBoardController;
    [SerializeField] private GeneratorController _generatorController;

    [Header("--- EVENTS ---")]
    [Header("RAISE")]
    [SerializeField] private CustomEvent_ElectricZone _onElectricityZoneEnabled;
    [SerializeField] private CustomEvent_ElectricZone _onElectricityZoneDisabled;
    [SerializeField] private CustomEvent _onElectricityShutdown;
    [SerializeField] private CustomEvent _onElectricityCameBack;

    [Header("GENERATOR LISTENERS")]
    [SerializeField] private CustomEvent _onGeneratorEnabled;
    [SerializeField] private CustomEvent _onGeneratorDisabled;
    [SerializeField] private CustomEvent _onGeneratorFuelEmpty;
    [SerializeField] private CustomEvent _onGeneratorFuelFilledFromEmpty;

    [Header("ELECTRIC TAB LISTENERS")]
    [SerializeField] private CustomEvent_ElectricZone _onSwitchBoardEnabled;
    [SerializeField] private CustomEvent_ElectricZone _onSwitchBoardDisabled;

    [Header("OTHER // INFOS")]
    [SerializeField] private List<GameZone> _currentEnabledZones;

    #endregion

    #region MONO CALLBACKS

    protected override void Awake()
    {
        base.Awake();
        //generateurs evt
        _onGeneratorFuelEmpty.handle += GeneratorController_OnGeneratorFuelEmpty;
        _onGeneratorFuelFilledFromEmpty.handle += GeneratorController_OnGeneratorFuelFilledFromEmpty;
        _onGeneratorEnabled.handle += GeneratorController_OnGeneratorEnabled;
        _onGeneratorDisabled.handle += GeneratorController_OnGeneratorDisabled;

        //tableau electrique evts
        _onSwitchBoardEnabled.handle += ElectricPannelController_OnElectricityEnabled;
        _onSwitchBoardDisabled.handle += ElectricPannelController_OnElectricityDisabled;
    }

    private void OnDestroy()
    {
        _onGeneratorFuelEmpty.handle -= GeneratorController_OnGeneratorFuelEmpty;
        _onGeneratorFuelFilledFromEmpty.handle -= GeneratorController_OnGeneratorFuelFilledFromEmpty;
        _onGeneratorEnabled.handle -= GeneratorController_OnGeneratorEnabled;
        _onGeneratorDisabled.handle -= GeneratorController_OnGeneratorDisabled;

        _onSwitchBoardEnabled.handle -= ElectricPannelController_OnElectricityEnabled;
        _onSwitchBoardDisabled.handle -= ElectricPannelController_OnElectricityDisabled;
    }

    #endregion

    #region DELEGATES

    #region Switchboard Events

    private void ElectricPannelController_OnElectricityDisabled(GameZone obj)
    {
        if (!_currentEnabledZones.Contains(obj)) return;
        _currentEnabledZones.Remove(obj);

        //si le gene est on et qu'on désactive
        if (_generatorController.IsOn)
        {
            _onElectricityZoneDisabled?.Raise(obj);
        }
    }

    private void ElectricPannelController_OnElectricityEnabled(GameZone obj)
    {
        if (_currentEnabledZones.Contains(obj)) return;
        _currentEnabledZones.Add(obj);

        //Quand on active un switch et que le générateur est allumé
        if (_generatorController.IsOn)
        {
            _onElectricityZoneEnabled?.Raise(obj);
        }
    }

    #endregion

    #region Generator Events

    private void GeneratorController_OnGeneratorEnabled()
    {
        //Remettre l'électricitée pour les boutons activés (get les zones)
        if (_generatorController.FuelValue > 0f && _switchBoardController.numberOfEnabledSwitch > 0)
        {
            _onElectricityCameBack?.Raise();

            foreach(GameZone zone in _currentEnabledZones)
            {
                _onElectricityZoneEnabled?.Raise(zone);
            }
        }
    }

    private void GeneratorController_OnGeneratorDisabled()
    {
        //On eleve l'électricitée dans toutes les zones normalement actives
        //Uniquement dans certaines zones
        if (_generatorController.FuelValue > 0f && _switchBoardController.numberOfEnabledSwitch > 0 && _generatorController.IsOn)
        {
            _onElectricityShutdown?.Raise();

            foreach (GameZone zone in _currentEnabledZones)
            {
                _onElectricityZoneDisabled?.Raise(zone);
            }
        }
    }

    private void GeneratorController_OnGeneratorFuelEmpty()
    {
        _switchBoardController.OnFuelEmpty();
        foreach(GameZone zone in _currentEnabledZones)
        {
            _onElectricityZoneDisabled?.Raise(zone);
        }
        _currentEnabledZones = new List<GameZone>();
    }

    private void GeneratorController_OnGeneratorFuelFilledFromEmpty()
    {
        _switchBoardController.OnFuelFilledFromEmpty();
    }

    #endregion

    #endregion
}

public class ElectricItem : ItemBase
{
    [Header("ELECTRIC ITEM")]
    public bool HasElectricity = false;

    public virtual void OnElecEnabled() { }

    public virtual void OnElecDisabled() { }
}