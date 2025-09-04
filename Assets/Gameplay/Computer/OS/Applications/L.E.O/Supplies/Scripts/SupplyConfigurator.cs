using AYellowpaper.SerializedCollections;
using LightHouse.Collections;
using LightHouse.Game.Computer.LEO.Supplies;
using UnityEngine;

[CreateAssetMenu(fileName = "SupplyConfig_", menuName = "New Supply Config")]
public class SupplyConfigurator : ScriptableObject
{
    public SerializedDictionary<E_SupplyCategory, SupplyItemDatas[]> SupplyConfig;
}
