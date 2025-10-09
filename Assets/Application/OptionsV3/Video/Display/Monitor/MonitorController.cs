using System.Collections.Generic;
using UnityEngine;

namespace LightHouse.Options.V3
{
    /// <summary>
    /// Contrôleur simple pour le choix du moniteur (Display 1/2/…).
    /// - UI: OptionEnum (gauche/droite) pour naviguer entre les écrans.
    /// - Apply(): déplace la fenêtre sur l'écran sélectionné.
    /// - Revert(): revient sur l'écran précédent + restaure la résolution/mode.
    /// </summary>
    public class MonitorController : MonoBehaviour, IOption
    {
        [Header("UI")]
        public OptionEnum optionEnum;

        // État interne
        private int _currentIndex;                 // où se trouve réellement la fenêtre
        private int _selectedIndex;                // choix dans l'UI
        private int _backupIndex;                  // pour revert

        private Vector2Int _backupResolution;
        private FullScreenMode _backupMode;

        private void Awake()
        {
            if (optionEnum == null)
            {
                Debug.LogError("[MonitorController] optionEnum est null.");
                enabled = false;
                return;
            }
        }

        private void OnEnable()
        {
            // brancher les boutons (ton OptionEnum ne publie pas forcément un event)
            optionEnum.LeftButton.onClick.AddListener(OnButtonClicked);
            optionEnum.RightButton.onClick.AddListener(OnButtonClicked);

            InitializeUI();
        }

        private void OnDisable()
        {
            optionEnum.LeftButton.onClick.RemoveListener(OnButtonClicked);
            optionEnum.RightButton.onClick.RemoveListener(OnButtonClicked);
        }

        private void InitializeUI()
        {
            // Récupère la liste des écrans
            var infos = new List<DisplayInfo>();
            Screen.GetDisplayLayout(infos);

            // Construit les labels: "Display i (WxH)"
            var labels = new List<string>();
            for (int i = 0; i < infos.Count; i++)
                labels.Add($"Display {i + 1} ({infos[i].width}x{infos[i].height})");

            // Fallback si rien (au cas où)
            if (labels.Count == 0)
                labels.Add($"Display 1 ({Screen.currentResolution.width}x{Screen.currentResolution.height})");

            // Alimente l'UI
            optionEnum.Choices = labels;
            optionEnum.ForceRebuildUI();

            // Trouve l'index courant à partir de mainWindowDisplayInfo
            _currentIndex = GetCurrentDisplayIndexByName(infos);
            _selectedIndex = _currentIndex;
            _backupIndex = _currentIndex;

            optionEnum.CurrentChoiceIndex = Mathf.Clamp(_currentIndex, 0, optionEnum.Choices.Count - 1);
            optionEnum.ForceRebuildUI();

            // Prépare le backup (pour Revert)
            _backupResolution = new Vector2Int(Screen.currentResolution.width, Screen.currentResolution.height);
            _backupMode = Screen.fullScreenMode;
        }

        private void OnButtonClicked()
        {
            // L’OptionEnum a déjà mis à jour CurrentChoiceIndex
            _selectedIndex = Mathf.Clamp(optionEnum.CurrentChoiceIndex, 0, optionEnum.Choices.Count - 1);
        }

        // ---------- IOption ----------

        public bool HasChanges() => _selectedIndex != _currentIndex;

        public void Apply()
        {
            if (!HasChanges()) return;

            // Sauvegarde pour Revert
            _backupIndex = _currentIndex;
            _backupResolution = new Vector2Int(Screen.currentResolution.width, Screen.currentResolution.height);
            _backupMode = Screen.fullScreenMode;

            var infos = new List<DisplayInfo>();
            Screen.GetDisplayLayout(infos);
            if (infos.Count == 0)
            {
                Debug.LogWarning("[MonitorController] Aucun display détecté.");
                return;
            }

            _selectedIndex = Mathf.Clamp(_selectedIndex, 0, infos.Count - 1);
            var target = infos[_selectedIndex];

            // Déplace la fenêtre sur l’écran choisi
            var op = Screen.MoveMainWindowTo(in target, Vector2Int.zero);
            op.completed += _ =>
            {
                // Sécurise la résolution (évite d’envoyer une res supérieure à la dalle cible)
                int targetW = Mathf.Min(_backupResolution.x, target.width);
                int targetH = Mathf.Min(_backupResolution.y, target.height);
                Screen.SetResolution(targetW, targetH, _backupMode);

                _currentIndex = _selectedIndex; // on est désormais sur ce display
                Debug.Log($"[MonitorController] Moved to display #{_currentIndex + 1} ({target.width}x{target.height})");
            };
        }

        public void Revert()
        {
            var infos = new List<DisplayInfo>();
            Screen.GetDisplayLayout(infos);
            if (infos.Count == 0)
            {
                Debug.LogWarning("[MonitorController] Revert: aucun display détecté.");
                return;
            }

            _backupIndex = Mathf.Clamp(_backupIndex, 0, infos.Count - 1);
            var back = infos[_backupIndex];

            var op = Screen.MoveMainWindowTo(in back, Vector2Int.zero);
            op.completed += _ =>
            {
                Screen.SetResolution(_backupResolution.x, _backupResolution.y, _backupMode);

                _selectedIndex = _backupIndex;
                _currentIndex = _backupIndex;

                // Refléter visuellement
                optionEnum.CurrentChoiceIndex = Mathf.Clamp(_currentIndex, 0, optionEnum.Choices.Count - 1);
                optionEnum.ForceRebuildUI();

                Debug.Log($"[MonitorController] Revert -> display #{_currentIndex + 1}");
            };
        }

        // ---------- Utilitaires ----------

        private static int GetCurrentDisplayIndexByName(List<DisplayInfo> infos)
        {
            if (infos == null || infos.Count == 0) return 0;

            var cur = Screen.mainWindowDisplayInfo;
            for (int i = 0; i < infos.Count; i++)
            {
                if (infos[i].name == cur.name)
                    return i;
            }
            return 0;
        }

        /// <summary>
        /// Si besoin, à appeler si les écrans changent à chaud (rebranche l’UI sans perdre l’état).
        /// </summary>
        public void RefreshUI()
        {
            int prevSelected = _selectedIndex;
            InitializeUI();
            _selectedIndex = Mathf.Clamp(prevSelected, 0, optionEnum.Choices.Count - 1);
            optionEnum.CurrentChoiceIndex = _selectedIndex;
            optionEnum.ForceRebuildUI();
        }
    }
}
