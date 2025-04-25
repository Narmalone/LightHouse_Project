using UnityEngine;
using UnityEngine.Localization.Settings;
using Random = UnityEngine.Random;

namespace LightHouse.Localization
{
    [System.Serializable]
    public enum InteractionsObjectsType
    {
        None,
        Switch,
        OpenClose,
        Grabable,
    }

    public enum InteractableObjectsName
    {
        Required,
        EnableGeneratorFirst
    }

    public class LocalizationManager : MonoBehaviour
    {
        void OnGUI()
        {
            if (GUI.Button(new Rect(10, 10, 150, 100), LocalizationSettings.SelectedLocale.Identifier.CultureInfo.DisplayName))
            {
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[Random.Range(0, LocalizationSettings.AvailableLocales.Locales.Count)];
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}

