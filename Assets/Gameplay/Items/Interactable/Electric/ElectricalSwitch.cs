using LightHouse.Electricity;
using UnityEngine;

namespace LightHouse.Items.Interactable
{
    public class ElectricalSwitch : InteractableSwitch
    {
        [SerializeField] private ElectricityZones _zoneToEnableOrDisable = ElectricityZones.None;
    }

}
