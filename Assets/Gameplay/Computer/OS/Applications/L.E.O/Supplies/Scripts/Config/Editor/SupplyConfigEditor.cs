using UnityEditor;
using UnityEngine;

namespace LightHouse.Game.Computer.LEO.Supplies
{
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
