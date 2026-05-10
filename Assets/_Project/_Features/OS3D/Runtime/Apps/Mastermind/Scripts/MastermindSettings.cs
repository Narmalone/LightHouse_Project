using UnityEngine;

namespace LightHouse.Features.Computer.Mastermind
{
    [System.Serializable]
    public struct MastermindColorEntry
    {
        public MastermindColor color;
        public Color32 visualColor;
    }

    [CreateAssetMenu(
        fileName = "SO_MastermindConfig_",
        menuName = ComputerAssetsMenuPaths.MastermindPath + "New Settings")]
    public class MastermindSettings : ScriptableObject
    {
        [Header("Gameplay")]

        public int CodeLength = 4;

        public int MaxAttempts = 12;

        public bool AllowDuplicateColors = true;

        [Header("Colors")]

        public MastermindColor[] AvailableColors;

        public MastermindColorEntry[] ColorEntries;

        #region Public API

        public Color32 GetVisualColor(
            MastermindColor color)
        {
            for (int i = 0;
                 i < ColorEntries.Length;
                 i++)
            {
                if (ColorEntries[i].color == color)
                {
                    return ColorEntries[i].visualColor;
                }
            }

            Debug.LogWarning(
                $"No visual color found for {color}");

            return Color.white;
        }

        #endregion
    }
}