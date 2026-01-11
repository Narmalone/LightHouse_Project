using LightHouse.Features.Computer.LEO.Supplies;
using UnityEditor;
using UnityEngine;

namespace LightHouse.Core.CustomEditors
{
    /// <summary>
    /// Script d'editor du Configurateur de Shop
    /// </summary>
    [CustomEditor(typeof(SO_SupplyConfigurator))]
    public class SupplyConfigEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var supplyConfig = (SO_SupplyConfigurator)target;
            if (GUILayout.Button("Set Unique ID'S"))
            {
                supplyConfig.SetAllIds();
                EditorUtility.SetDirty(supplyConfig);
            }
        }
    }
}
