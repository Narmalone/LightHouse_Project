using UnityEditor;
using UnityEngine;

namespace LightHouse.Core.Save
{
    [CustomEditor(typeof(SaveLoadSystem))]
    public class SaveManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            SaveLoadSystem _saveLoadSystem = (SaveLoadSystem)target;

            string gameName = _saveLoadSystem.GameData.Name;
            DrawDefaultInspector();

            if(GUILayout.Button("New Game"))
            {
                _saveLoadSystem.NewGame();
            }

            if (GUILayout.Button("Save Game"))
            {
                _saveLoadSystem.SaveGame();
                Debug.Log("Game saved");
            }

            if (GUILayout.Button("Load Game"))
            {
                _saveLoadSystem.LoadGame(gameName);
            }

            if (GUILayout.Button("Delete Game"))
            {
                _saveLoadSystem.DeleteGame(gameName);
            }

            if (GUILayout.Button("Delete All Games"))
            {
                _saveLoadSystem.DeleteAllGames();
            }
        }
    }
}
