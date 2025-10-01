using UnityEngine;

namespace LightHouse.Game.Computer.LEO.Supplies
{
    /// <summary>
    /// Données d'un item de ravitaillement (ID, nom, coût) + états runtime (stock/select).
    /// </summary>
    [System.Serializable]
    public class SupplyItemDatas
    {
        #region ===== Serialized (design-time) =====

        [Tooltip("Identifiant unique stable de l'item (clé logique).")]
        public int UniqueID;

        [Tooltip("Nom affiché dans l'UI.")]
        public string Name;

        [Min(0f)]
        [Tooltip("Prix unitaire de l'item.")]
        public float Cost;

        //TO DO::
        //Ajouter le Prefab qui sera instantié...
        public GameObject Prefab;

        #endregion

        #region ===== Runtime-only (non sérialisé) =====

        [System.NonSerialized]
        public float CurrentStockCount; // stock courant

        [System.NonSerialized]
        public int SelectedAmountToBuy; // quantité sélectionnée 

        #endregion

        #region ===== Helpers / Convenience API (facultatif) =====

        /// <summary>
        /// Coût total courant de la sélection (clampée >= 0).
        /// </summary>
        public float SelectedTotalCost =>
            Mathf.Max(0, SelectedAmountToBuy) * Cost;

        /// <summary>
        /// Remet à zéro les champs runtime (sélection, stock).
        /// </summary>
        public void ResetRuntime()
        {
            SelectedAmountToBuy = 0;
            CurrentStockCount = 0f;
        }

        public override string ToString()
            => $"[SupplyItemDatas] ID={UniqueID} Name='{Name}' Cost={Cost:0.##} Selected={SelectedAmountToBuy} Stock={CurrentStockCount:0.##}";

        #endregion
    }
}
