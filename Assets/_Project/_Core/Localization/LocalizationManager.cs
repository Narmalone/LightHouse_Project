using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LightHouse.Core.Localization
{
    [System.Serializable]
    public enum InteractionsObjectsType
    {
        None,
        Switch,
        OpenClose,
        Grabable,
    }

    public class LocalizationManager : PersistentSingleton<LocalizationManager>
    {
        protected override void Awake()
        {
            base.Awake();
            LocalizationSettings.SelectedLocaleChanged += LocalizationSettings_SelectedLocaleChanged;
        }

        private void OnDestroy()
        {
            LocalizationSettings.SelectedLocaleChanged -= LocalizationSettings_SelectedLocaleChanged;
        }

        private void LateUpdate()
        {
            /*if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[0];
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[1];
            }*/
        }


        /// <summary>
        /// Routine générique : attend n'importe quel AsyncOperationHandle<T> 
        /// et invoque le callback avec le résultat.
        /// </summary>
        public IEnumerator GetHandleRoutine<T>(AsyncOperationHandle<T> handle, Action<T> onComplete, string errorLabel = null)
        {
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                onComplete?.Invoke(handle.Result);
            }
            else
            {
                Debug.LogError($"Échec chargement {errorLabel ?? typeof(T).Name} : {handle.OperationException}");
            }
        }

        public IEnumerator GetStringRoutine(LocalizedString targetString, Action<string> onComplete)
        {
            yield return GetHandleRoutine(targetString.GetLocalizedStringAsync(), onComplete, "string localisée");
        }

        public IEnumerator GetAssetRoutine<T>(LocalizedAsset<T> targetAsset, Action<T> onComplete) where T : UnityEngine.Object
        {
            yield return GetHandleRoutine(targetAsset.LoadAssetAsync(), onComplete, $"asset localisé ({typeof(T).Name})");
        }

        private void LocalizationSettings_SelectedLocaleChanged(UnityEngine.Localization.Locale obj)
        {
            Debug.Log(obj.LocaleName);
        }
    }
}

