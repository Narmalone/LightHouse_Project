using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace LightHouse.Game.Options
{
    public static class DisplaySettingManager
    {
        public static event Action OnDisplayChanged;

        private static int previousDisplayIndex;
        private static Vector2Int previousResolution;
        private static FullScreenMode previousFullScreenMode;

        public static void ApplyDisplayChange(int newDisplayIndex)
        {
            SaveCurrentState();

            List<DisplayInfo> displays = new();
            Screen.GetDisplayLayout(displays);

            if (newDisplayIndex < displays.Count)
            {
                var displayInfo = displays[newDisplayIndex];
                var move = Screen.MoveMainWindowTo(in displayInfo, new Vector2Int(0, 0));
                move.completed += (op) =>
                {
                    Debug.Log($"Moved to display {newDisplayIndex}");
                    OnDisplayChanged?.Invoke();
                };
            }
        }

        public static void RevertDisplayChange()
        {
            List<DisplayInfo> displays = new();
            Screen.GetDisplayLayout(displays);

            if (previousDisplayIndex < displays.Count)
            {
                var displayInfo = displays[previousDisplayIndex];
                var move = Screen.MoveMainWindowTo(in displayInfo, new Vector2Int(0, 0));
                move.completed += (op) =>
                {
                    Screen.SetResolution(previousResolution.x, previousResolution.y, previousFullScreenMode);
                    Debug.Log("Reverted to previous display and resolution.");
                    OnDisplayChanged?.Invoke();
                };
            }
        }

        public static void SaveCurrentState()
        {
            previousDisplayIndex = GetCurrentDisplayIndex();
            previousResolution = new Vector2Int(ResolutionSetting.CurrentResolution.x, ResolutionSetting.CurrentResolution.y);
            previousFullScreenMode = Screen.fullScreenMode;
        }

        public static int GetCurrentDisplayIndex()
        {
            List<DisplayInfo> displays = new();
            Screen.GetDisplayLayout(displays);
            var current = Screen.mainWindowDisplayInfo;

            for (int i = 0; i < displays.Count; i++)
            {
                if (displays[i].name == current.name)
                    return i;
            }

            return 0;
        }
    }
}
