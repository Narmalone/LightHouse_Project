using UnityEngine;
using UnityEngine.Localization.Settings;

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

    public class LocalizationManager : MonoBehaviour
    {
        private void Awake()
        {
            LocalizationSettings.SelectedLocaleChanged += LocalizationSettings_SelectedLocaleChanged;
        }

        private void OnDestroy()
        {
            LocalizationSettings.SelectedLocaleChanged -= LocalizationSettings_SelectedLocaleChanged;
        }

        private void LateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                //LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[0];
            }
        }

        private void LocalizationSettings_SelectedLocaleChanged(UnityEngine.Localization.Locale obj)
        {
            Debug.Log(obj.LocaleName);
        }
    }
}

