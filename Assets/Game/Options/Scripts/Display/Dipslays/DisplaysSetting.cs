using System;
using System.Collections.Generic;
using UnityEngine;

namespace LightHouse.Game.Options
{
    public class DisplaysSetting : IOptionSetting
    {
        public static event Action OnDisplayScreenChanged;
        private Vector2Int backupResolution;
        private FullScreenMode backupFullScreenMode;

        public static bool IsRevertingDisplay = false;
        private int initialDisplay;
        private int currentDisplay;
        private int selectedDisplay;
        private int backupDisplay; // 💬 Ajout : on garde une sauvegarde avant Apply !

        public int SelectedDisplay => selectedDisplay;

        public static int SSelectedDisplay = -1;

        public DisplaysSetting()
        {
            initialDisplay = 0; // Toujours Display 0 (Display 1 dans Unity)
            currentDisplay = initialDisplay;
            selectedDisplay = initialDisplay;
            backupDisplay = initialDisplay;
            SSelectedDisplay = initialDisplay;
        }

        public void SetSelectedDisplay(int displayIndex)
        {
            selectedDisplay = displayIndex;
            SSelectedDisplay = displayIndex;
        }

        public bool HasChanged()
        {
            return currentDisplay != selectedDisplay;
        }

        public void Apply()
        {
            if (selectedDisplay < Display.displays.Length)
            {
                // 🔥 Avant de bouger, sauver où on était !
                backupDisplay = currentDisplay;
                backupResolution = new Vector2Int(ResolutionSetting.CurrentResolution.x, ResolutionSetting.CurrentResolution.y);
                backupFullScreenMode = Screen.fullScreenMode;

                List<DisplayInfo> displaysLayout = new List<DisplayInfo>();
                Screen.GetDisplayLayout(displaysLayout);

                if (selectedDisplay < displaysLayout.Count)
                {
                    var displayInfo = displaysLayout[selectedDisplay];
                    var s = Screen.MoveMainWindowTo(in displayInfo, new Vector2Int(0, 0));
                    s.completed += (ss) =>
                    {
                        Debug.Log($"Fenêtre déplacée, forçage de résolution sur Display {selectedDisplay}");

                        Screen.SetResolution(ResolutionSetting.CurrentResolution.x, ResolutionSetting.CurrentResolution.y, Screen.fullScreenMode);

                        OnDisplayScreenChanged?.Invoke();
                    };

                    currentDisplay = selectedDisplay; // ➔ OK maintenant
                    Debug.Log($"Display {selectedDisplay + 1} activé et fenêtre déplacée !");
                }
                else
                {
                    Debug.LogWarning($"SelectedDisplay {selectedDisplay} hors de range ({displaysLayout.Count} displays)");
                }
            }
            else
            {
                Debug.LogWarning("Display index hors limites !");
            }
        }

        public void Revert()
        {
            IsRevertingDisplay = true;

            List<DisplayInfo> displaysLayout = new List<DisplayInfo>();
            Screen.GetDisplayLayout(displaysLayout);

            if (backupDisplay < displaysLayout.Count)
            {
                var displayInfo = displaysLayout[backupDisplay];
                var s = Screen.MoveMainWindowTo(in displayInfo, new Vector2Int(0, 0));
                s.completed += (ss) =>
                {
                    Debug.Log($"Fenêtre revertée sur Display {backupDisplay}");

                    // Restaurer ancienne résolution + mode
                    Screen.SetResolution(backupResolution.x, backupResolution.y, backupFullScreenMode);

                    // ✅ APRES avoir tout restauré, on rafraîchit toute l'UI
                    OnDisplayScreenChanged?.Invoke(); // ➔ ça va appeler ton InitializeControllers()

                    IsRevertingDisplay = false;
                };

                selectedDisplay = backupDisplay;
                currentDisplay = backupDisplay;
            }
            else
            {
                Debug.LogWarning($"BackupDisplay {backupDisplay} hors de range ({displaysLayout.Count} displays)");
            }
        }


        public IOptionSetting GetSetting()
        {
            return this;
        }
    }
}
