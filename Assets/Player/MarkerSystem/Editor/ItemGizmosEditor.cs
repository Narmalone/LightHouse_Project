using UnityEngine;
using UnityEditor;
using LightHouse.Items.Inventory;
using LightHouse.Items.Interactable;

namespace LightHouse.CustomEditors
{
    [InitializeOnLoad]
    public static class ItemsGizmosEditor
    {
        static ItemsGizmosEditor()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        static void OnSceneGUI(SceneView sceneView)
        {
            if (GizmoDisplaySettings.ShowInventory)
                DrawInventoryGizmos();

            if (GizmoDisplaySettings.ShowInteractables)
                DrawInteractableGizmos();
        }

        static void DrawInventoryGizmos()
        {
            var items = GameObject.FindObjectsByType<InventoryItemBase>(FindObjectsSortMode.None);
            foreach (var item in items)
            {
                if (item == null) continue;
                DrawGizmo(item.transform.position, ItemRole.Inventory);
            }
        }

        static void DrawInteractableGizmos()
        {
            var items = GameObject.FindObjectsByType<InteractableItemBase>(FindObjectsSortMode.None);
            foreach (var item in items)
            {
                if (item == null) continue;
                DrawGizmo(item.transform.position, ItemRole.Interactable);
            }
        }

        static void DrawGizmo(Vector3 pos, ItemRole role)
        {
            Color color = GetColorForRole(role);
            Handles.color = color;
            Handles.DrawWireDisc(pos, Vector3.up, 0.5f);

            GUIStyle style = new GUIStyle();
            style.normal.textColor = color;
            style.fontStyle = FontStyle.Bold;
            Handles.Label(pos + Vector3.up * 0.7f, role.ToString(), style);
        }

        static Color GetColorForRole(ItemRole role)
        {
            switch (role)
            {
                case ItemRole.Inventory: return Color.yellow;
                case ItemRole.Interactable: return Color.green;
                default: return Color.gray;
            }
        }
    }


    public static class GizmoDisplaySettings
    {
        public static bool ShowInventory = true;
        public static bool ShowInteractables = true;
    }

}
