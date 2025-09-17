using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace LightHouse.Game.Computer.LEO.Supplies
{
    /// <summary>
    /// Base de donnée permettant de configurer le Shop / ravitaillement
    /// </summary>
    [CreateAssetMenu(fileName = "SupplyConfig_", menuName = "New Supply Config")]
    public class SO_SupplyConfigurator : ScriptableObject
    {
        public SerializedDictionary<E_SupplyCategory, SupplyItemDatas[]> SupplyConfig;

        public void SetAllIds()
        {
            int i = 0;

            // Itère uniquement sur les valeurs (les tableaux), pas besoin des clés
            foreach (var array in SupplyConfig.Values)
            {
                if (array == null) continue; // sécurité si un tableau est null
                for (int j = 0; j < array.Length; j++)
                {
                    array[j].UniqueID = i++;
                }
            }
        }

    }

}
