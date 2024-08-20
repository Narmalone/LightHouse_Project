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
            room.GetAllElecItemInRoom();
        }
        if (GUILayout.Button("Debug"))
        {
            room.DebugForTest();
        }
    }
}
