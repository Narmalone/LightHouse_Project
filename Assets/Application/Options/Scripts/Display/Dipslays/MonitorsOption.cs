using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace LightHouse.Game.Options
{
    /// <summary>
    /// MonoBehaviour unique : gère la liste des moniteurs via OptionEnum
    /// + stocke le choix dans DisplaysSetting
    /// + applique/revert sur demande (boutons Apply/Revert).
    /// Aucune boucle d'événements : l'UI n'applique rien d'elle-même.
    /// </summary>
    public class MonitorsOptions : MonoBehaviour
    {
        /*[Header("UI")]
        [SerializeField] private OptionEnum optionEnum;
        [SerializeField] private TMP_Text label;

        [Header("Texts")]
        [SerializeField] private LocalizedString displayName; // ex: "Écran"

        private readonly List<DisplayInfo> _displayInfos = new();
        private MonitorSetting _setting;

        private void Start()
        {
            _setting = new MonitorSetting();

            RebuildOptions();

            // Sélectionne l’écran courant (visuel) + modèle
            int currentIndex = Mathf.Clamp(MonitorSetting.GetCurrentDisplayIndex(), 0, optionEnum.Choices.Count - 1);
            SafeSetSelectedIndex(currentIndex);
            _setting.SetSelectedDisplay(currentIndex);

            // Listener utilisateur : écrit juste dans le modèle
            SubscribeOptionChanged();

            // Si le système change (après Apply/Revert), on reflète visuellement
            MonitorSetting.OnDisplayScreenChanged += OnSystemDisplayChanged;

            // Label
            if (label != null && displayName != null)
                label.text = displayName.GetLocalizedString();
        }

        private void OnDestroy()
        {
            MonitorSetting.OnDisplayScreenChanged -= OnSystemDisplayChanged;
            UnsubscribeOptionChanged();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                Apply();
            }
        }

        // ------- Boutons -------

        public void Apply() => _setting?.Apply();
        public void Revert() => _setting?.Revert();

        // ------- Internes -------

        private void RebuildOptions()
        {
            _displayInfos.Clear();
            Screen.GetDisplayLayout(_displayInfos);

            var options = new List<string>();
            string baseName = "Display";

            for (int i = 0; i < _displayInfos.Count; i++)
            {
                var info = _displayInfos[i];
                options.Add($"{baseName} {i + 1} ({info.width}x{info.height})");
            }

            if (options.Count == 0)
                options.Add($"{baseName} 1 ({Screen.currentResolution.width}x{Screen.currentResolution.height})");

            optionEnum.Choices.Clear();
            optionEnum.AddOptions(options);
        }

        private void OnOptionChanged(int newIndex)
        {
            // L’utilisateur a changé : on stocke le choix, on n’applique PAS ici
            _setting.SetSelectedDisplay(newIndex);
        }

        private void OnSystemDisplayChanged()
        {
            // Le système a bougé la fenêtre (Apply/Revert) → refléter visuellement + modèle
            int idx = Mathf.Clamp(MonitorSetting.GetCurrentDisplayIndex(), 0, optionEnum.Choices.Count - 1);
            SafeSetSelectedIndex(idx);
            _setting.SetSelectedDisplay(idx);
        }

        // --- Compat OptionEnum ---

        // Si ton OptionEnum expose SetSelectedIndex(int) et un event OnValueChanged(int),
        // ces helpers utiliseront directement cette API.
        // Sinon, ils basculent en mode "silencieux" CurrentChoiceIndex + ForceRebuildUI + écoute des boutons.

        private bool _usingDirectEventAPI;

        private void SubscribeOptionChanged()
        {
            // Détecte si l’API directe existe (selon ta version d’OptionEnum)
            // -> Si tu n'as PAS OnValueChanged dans ta classe, supprime simplement le try/catch et la partie else.
            try
            {
                optionEnum.OnValueChanged -= OnOptionChanged;
                optionEnum.OnValueChanged += OnOptionChanged;
                _usingDirectEventAPI = true;
            }
            catch
            {
                // Fallback: on écoute les clics boutons (évite les lambdas anonymes pour RemoveListener)
                optionEnum.LeftButton.onClick.AddListener(OnButtonClicked);
                optionEnum.RightButton.onClick.AddListener(OnButtonClicked);
                _usingDirectEventAPI = false;
            }
        }

        private void UnsubscribeOptionChanged()
        {
            if (_usingDirectEventAPI)
            {
                try { optionEnum.OnValueChanged -= OnOptionChanged; } catch { *//* ignore *//* }
            }
            else
            {
                optionEnum.LeftButton.onClick.RemoveListener(OnButtonClicked);
                optionEnum.RightButton.onClick.RemoveListener(OnButtonClicked);
            }
        }

        private void OnButtonClicked()
        {
            OnOptionChanged(optionEnum.CurrentChoiceIndex);
        }

        private void SafeSetSelectedIndex(int index)
        {
            // Utilise SetSelectedIndex si disponible (évite de déclencher OnValueChanged si c'est silencieux)
            try
            {
                optionEnum.SetSelectedIndex(index);
            }
            catch
            {
                optionEnum.CurrentChoiceIndex = index;
                optionEnum.ForceRebuildUI();
            }
        }
    }*/
    }
}
