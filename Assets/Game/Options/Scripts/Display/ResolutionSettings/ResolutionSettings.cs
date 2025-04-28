using UnityEngine;

namespace LightHouse.Game.Options
{
    public class ResolutionSetting : IOptionSetting
    {
        private Vector2Int initialResolution;
        private Vector2Int currentResolution;
        private Vector2Int selectedResolution;

        public static Vector2Int CurrentResolution;

        public ResolutionSetting()
        {
            initialResolution = new Vector2Int(Screen.width, Screen.height);
            currentResolution = initialResolution;
            CurrentResolution = initialResolution;
            selectedResolution = initialResolution;
        }

        public void SetSelectedResolution(Vector2Int resolution)
        {
            selectedResolution = resolution;
        }

        public bool HasChanged()
        {
            return !currentResolution.Equals(selectedResolution);
        }

        public void Apply()
        {
            Screen.SetResolution(selectedResolution.x, selectedResolution.y, Screen.fullScreenMode);
            currentResolution = selectedResolution;
            CurrentResolution = selectedResolution;
            Debug.Log("resolution applied");
        }

        public void Revert()
        {
            Screen.SetResolution(currentResolution.x, currentResolution.y, Screen.fullScreenMode);
        }

        public IOptionSetting GetSetting()
        {
            return this;
        }
    }
}
