using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Localization;

namespace LightHouse.Game.Options
{
    /// <summary>
    /// Contrôle un TMP_Dropdown (Canvas) listant les écrans disponibles.
    /// </summary>
    public class DisplaysDropdownController
    {
        private readonly TMP_Dropdown _dropdown;                         // uGUI TMP_Dropdown
        private readonly ConfirmationPopupController _popup;             // Votre popup confirm/cancel/timeout
        private readonly LocalizedString _displayName;                   // "Écran", "Display", etc.
        private readonly TMP_Text _label;                                // Label séparé (facultatif)

        private readonly DisplaysSetting _displaysSetting;               // Votre modèle (pour Apply/HasChanged)
        private readonly List<DisplayInfo> _displayInfos = new();        // Cache de la topologie d’écrans

        public DisplaysDropdownController(
            TMP_Dropdown dropdown,
            ConfirmationPopupController confirmationPopupController,
            LocalizedString displayName,
            TMP_Text localizedLabel = null)
        {
            _dropdown = dropdown;
            _popup = confirmationPopupController;
            _displayName = displayName;
            _label = localizedLabel;
            _displaysSetting = new DisplaysSetting();
        }

        /// <summary>
        /// Remplit le dropdown et accroche les callbacks.
        /// </summary>
        public void Initialize()
        {
            if (_dropdown == null)
            {
                Debug.LogError("[DisplaysDropdownController] TMP_Dropdown est null.");
                return;
            }

            RebuildOptions();

            // Sélectionne l’écran courant
            int currentIndex = Mathf.Clamp(DisplaySettingManager.GetCurrentDisplayIndex(), 0, _dropdown.options.Count - 1);

            _dropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
            _dropdown.value = currentIndex;
            _dropdown.RefreshShownValue();
            _dropdown.onValueChanged.AddListener(OnDropdownValueChanged);

            // Pousse l’état initial vers le modèle (si besoin)
            UpdateSettingFromIndex(_dropdown.value);
        }

        /// <summary>
        /// Met à jour le libellé et les options localisées (ex: "Écran 1", "Display 1").
        /// </summary>
        public void UpdateLanguage()
        {
            if (_label != null && _displayName != null)
                _label.text = _displayName.GetLocalizedString();

            // Les intitulés des options dépendent de la localisation -> on reconstruit
            int prevIndex = _dropdown != null ? _dropdown.value : 0;
            RebuildOptions();
            _dropdown.SetValueWithoutNotify(Mathf.Clamp(prevIndex, 0, _dropdown.options.Count - 1));
            _dropdown.RefreshShownValue();
        }

        /// <summary>
        /// Applique réellement le changement d’écran via votre modèle.
        /// </summary>
        public void Apply()
        {
            if (_displaysSetting.HasChanged())
                _displaysSetting.Apply();
        }

        // ---------- Internals ----------

        private void RebuildOptions()
        {
            _displayInfos.Clear();
            Screen.GetDisplayLayout(_displayInfos); // Remplit la liste avec la topologie d’écrans

            var opts = new List<TMP_Dropdown.OptionData>(_displayInfos.Count);
            string baseName = _displayName != null ? _displayName.GetLocalizedString() : "Display";

            for (int i = 0; i < _displayInfos.Count; i++)
            {
                var info = _displayInfos[i];
                // Exemple: "Écran 1 (1920x1080)"
                opts.Add(new TMP_Dropdown.OptionData($"{baseName} {i + 1} ({info.width}x{info.height})"));
            }

            if (opts.Count == 0)
            {
                // Fallback si API ne renvoie rien (rare) : on montre au moins l’écran principal
                opts.Add(new TMP_Dropdown.OptionData($"{baseName} 1 ({Screen.currentResolution.width}x{Screen.currentResolution.height})"));
            }

            _dropdown.options = opts;
        }

        private void OnDropdownValueChanged(int newIndex)
        {
            // Si on sélectionne le même index, on ne fait rien
            int current = Mathf.Clamp(DisplaySettingManager.GetCurrentDisplayIndex(), 0, _dropdown.options.Count - 1);
            if (newIndex == current)
            {
                UpdateSettingFromIndex(newIndex);
                return;
            }

            // Applique "provisoirement" le nouvel écran (si c’est le fonctionnement désiré)
            DisplaySettingManager.ApplyDisplayChange(newIndex);
            UpdateSettingFromIndex(newIndex);

            // Demande confirmation
            if (_popup != null)
            {
                _popup.Show(
                    confirmAction: () =>
                    {
                        // Confirmé : on laisse tel quel (optionnel: pousser dans Settings persistent)
                        Debug.Log("Display change confirmed.");
                    },
                    cancelAction: () =>
                    {
                        // Annulé : on revient à l’écran précédent
                        DisplaySettingManager.RevertDisplayChange();
                        // Re-synchronise le dropdown
                        int idx = Mathf.Clamp(DisplaySettingManager.GetCurrentDisplayIndex(), 0, _dropdown.options.Count - 1);
                        _dropdown.SetValueWithoutNotify(idx);
                        _dropdown.RefreshShownValue();
                        UpdateSettingFromIndex(idx);
                    },
                    timeOutAction: 15 // selon votre signature: timeout en secondes
                );
            }
        }

        private void UpdateSettingFromIndex(int index)
        {
            // Si votre DisplaysSetting stocke “l’écran cible”
            _displaysSetting.SetSelectedDisplay(index);
        }
    }
}
