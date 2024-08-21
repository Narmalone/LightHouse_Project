using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Room))]
public class RoomEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Room room = (Room)target;
        if (GUILayout.Button("Get All Elecs Items In Room"))
        {
            GetAllElecItemInRoom(room);
        }
    }

    public void GetAllElecItemInRoom(Room room)
    {
        room.ElectricityItems = new List<ElectricItem>();
        for (int i = 0; i < room._electricityItemParent.childCount; i++)
        {
            if(room._electricityItemParent.GetChild(i).TryGetComponent(out ElectricItem item))
            {
                room.ElectricityItems.Add(item);
            }
        }
    }
}
