using System;
using System.Collections.Generic;
using UnityEngine;

public enum ElectricityZones
{
    OutsideLocal,
    Bathroom,
    BedRoom,
    KitchenAndOther,
    Office,
    Lens
}

public class ElectricityManager : MonoBehaviour
{
    //ENlever le syst ou quand y'a plus de fuel ça shutdown et plus tot ça ne fais juste plus passer le courant
    //Faire en sorte que lorsque la mise à jour des zones actives soit bonne
    //trouver un moyen afin d'appeler de manière optimale l'event désactiver / activer objet niv électrique
    [SerializeField] private ElectricPannelController electricPannelController;
    [SerializeField] private GeneratorController generatorController;

    public static event Action OnElectricityShutDown;
    public static event Action OnElectricityCameBack;

    public List<ElectricityZones> CurrentEnabledZones;

    private void Awake()
    {
        generatorController.OnGeneratorFuelEmpty += GeneratorController_OnGeneratorFuelEmpty;
        generatorController.OnGeneratorFuelFilledFromEmpty += GeneratorController_OnGeneratorFuelFilledFromEmpty;
        generatorController.OnGeneratorEnabled += GeneratorController_OnGeneratorEnabled;
        generatorController.OnGeneratorDisabled += GeneratorController_OnGeneratorDisabled;

        electricPannelController.OnElectricityEnabled += ElectricPannelController_OnElectricityEnabled;
        electricPannelController.OnElectricityDisabled += ElectricPannelController_OnElectricityDisabled;
    }

    private void ElectricPannelController_OnElectricityDisabled(ElectricityZones obj)
    {
        if (!CurrentEnabledZones.Contains(obj)) return;
        CurrentEnabledZones.Remove(obj);   
    }

    private void ElectricPannelController_OnElectricityEnabled(ElectricityZones obj)
    {
        if (CurrentEnabledZones.Contains(obj)) return;
        CurrentEnabledZones.Add(obj);
    }

    private void GeneratorController_OnGeneratorEnabled()
    {
        //Remettre l'électricitée pour les boutons activés (get les zones)
        if (generatorController.FuelValue > 0f && electricPannelController.numberOfEnabledSwitch > 0)
        {
            OnElectricityCameBack?.Invoke();
        }
    }

    private void GeneratorController_OnGeneratorDisabled()
    {
        //On eleve l'électricitée dans toutes les zones normalement actives
        //Uniquement dans certaines zones
        if (generatorController.FuelValue > 0f && electricPannelController.numberOfEnabledSwitch > 0 && generatorController.IsOn)
        {
            OnElectricityShutDown?.Invoke();
        }
    }

    private void OnDestroy()
    {
        generatorController.OnGeneratorFuelEmpty -= GeneratorController_OnGeneratorFuelEmpty;
        generatorController.OnGeneratorFuelFilledFromEmpty -= GeneratorController_OnGeneratorFuelFilledFromEmpty;
        generatorController.OnGeneratorEnabled -= GeneratorController_OnGeneratorEnabled;
        generatorController.OnGeneratorDisabled -= GeneratorController_OnGeneratorDisabled;

        electricPannelController.OnElectricityEnabled -= ElectricPannelController_OnElectricityEnabled;
        electricPannelController.OnElectricityDisabled -= ElectricPannelController_OnElectricityDisabled;
    }

    private void GeneratorController_OnGeneratorFuelFilledFromEmpty()
    {
        electricPannelController.OnFuelFilledFromEmpty();
    }

    private void GeneratorController_OnGeneratorFuelEmpty()
    {
        electricPannelController.OnFuelEmpty();

    }
}

public interface IElectricityEvent
{
    void OnElecEnable();
    void OnElecDisable();
}