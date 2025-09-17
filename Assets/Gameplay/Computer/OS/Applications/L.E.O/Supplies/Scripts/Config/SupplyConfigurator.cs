using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace LightHouse.Game.Computer.LEO.Supplies
{
    /// <summary>
    /// Base de donnée permettant de configurer le Shop / ravitaillement
    /// </summary>
    [CreateAssetMenu(fileName = "SupplyConfig_", menuName = "New Supply Config")]
    public class SupplyConfigurator : ScriptableObject
    {
        public SerializedDictionary<E_SupplyCategory, SupplyItemDatas[]> SupplyConfig;
    }

}
