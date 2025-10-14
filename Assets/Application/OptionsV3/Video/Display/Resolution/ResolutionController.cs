using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace LightHouse.Options.V3
{
    public class ResolutionController : MonoBehaviour, IOption
    {
        [Header("UI")]
        public TMP_Dropdown _resolutionDropdown;

        [Header("State (committed)")]
        public Vector2Int CurrentResolution;   // état committé (appliqué)
        public Vector2Int BackupResolution;    // miroir du committé pour revert

        // --- internes ---
        private readonly List<Vector2Int> _choices = new(); // triées desc
        private bool _suppressDropdownCb;
        private DisplayKey _displayKey; // pour détecter un changement d’écran

        private void Awake()
        {
            if (!_resolutionDropdown)
            {
                Debug.LogError("[ResolutionController] TMP_Dropdown manquant.");
                enabled = false;
                return;
            }
        }

        private void OnEnable()
        {
            // Brancher le callback
            _resolutionDropdown.onValueChanged.AddListener(OnDropdownChanged);

            // Seed: construire depuis le moniteur courant
            RebuildForCurrentMonitor(selectHighest: false); // on seed sur la rés système
        }

        private void OnDisable()
        {
            _resolutionDropdown.onValueChanged.RemoveListener(OnDropdownChanged);
        }

        private void Update()
        {
            // Détections passives d’un changement de moniteur (ex: MonitorController a bougé la fenêtre)
            var keyNow = MakeKey(Screen.mainWindowDisplayInfo);
            if (!_displayKey.Equals(keyNow))
            {
                _displayKey = keyNow;
                // Rebuild et sélectionne la plus élevée quand l’écran change
                RebuildForCurrentMonitor(selectHighest: true);
            }
        }

        // --- IOption ---

        public bool HasChanges()
        {
            // comparaison sélection UI (pending) vs committé (CurrentResolution)
            var selected = GetSelectedResolutionFromUI();
            return selected != CurrentResolution;
        }

        public void Apply()
        {
            if (!Application.isPlaying) return;

            var selected = GetSelectedResolutionFromUI();
            if (!HasChanges()) return;

            // On garde le mode actuel (DisplayModeController s’occupe du mode)
            var mode = Screen.fullScreenMode;

            // Sécurité: clamp à la taille du moniteur cible (devrait déjà être OK si vient de Screen.resolutions)
            var di = Screen.mainWindowDisplayInfo;
            int w = Mathf.Min(selected.x, di.width);
            int h = Mathf.Min(selected.y, di.height);

            Screen.SetResolution(w, h, mode);

            // Commit
            CurrentResolution = new Vector2Int(w, h);
            BackupResolution = CurrentResolution;

            // Align UI sur le committé
            SelectInDropdown(CurrentResolution);
            Debug.Log($"[ResolutionController] Apply → {w}x{h} | mode:{mode}");
        }

        public void Revert()
        {
            if (!Application.isPlaying) return;

            // Revenir à la dernière résolution committée
            var mode = Screen.fullScreenMode;
            Screen.SetResolution(BackupResolution.x, BackupResolution.y, mode);

            CurrentResolution = BackupResolution;

            // UI → committé
            SelectInDropdown(CurrentResolution);
            Debug.Log($"[ResolutionController] Revert → {CurrentResolution.x}x{CurrentResolution.y} | mode:{mode}");
        }

        // --- Public helper si tu veux l’appeler explicitement depuis MonitorController après Apply() ---
        public void RebuildForCurrentMonitor(bool selectHighest = true)
        {
            BuildChoicesForCurrentMonitor();

            if (_choices.Count == 0)
            {
                // Fallback: utiliser la résolution système
                var r = Screen.currentResolution;
                CurrentResolution = new Vector2Int(r.width, r.height);
                BackupResolution = CurrentResolution;

                _resolutionDropdown.ClearOptions();
                _resolutionDropdown.AddOptions(new List<string> { $"{r.width} x {r.height}" });
                _resolutionDropdown.value = 0;
                _resolutionDropdown.RefreshShownValue();
                return;
            }

            // Résolution système comme point de vérité pour le seed (sauf si on force la plus haute)
            var sys = Screen.currentResolution;
            var sysRes = new Vector2Int(sys.width, sys.height);

            // Choix de sélection
            Vector2Int toSelect = selectHighest ? _choices[0] : (FindClosestOrExact(sysRes) ?? _choices[0]);

            // Mettre à jour notre committé (on NE change pas la résolution système ici)
            CurrentResolution = toSelect;
            BackupResolution = CurrentResolution;

            // Remplir l’UI et sélectionner
            FillDropdownLabels();
            SelectInDropdown(toSelect);

            //Debug.Log($"[ResolutionController] RebuildForCurrentMonitor → {toSelect.x}x{toSelect.y}  (highest={selectHighest})");
        }

        // --- Internes: build / UI ---

        private void BuildChoicesForCurrentMonitor()
        {
            _choices.Clear();

            // Clé d’écran courante
            _displayKey = MakeKey(Screen.mainWindowDisplayInfo);

            // Récupérer les résolutions disponibles pour ce moniteur (Unity renvoie des doublons par refresh rate)
            var resArray = Screen.resolutions;
            var set = new HashSet<Vector2Int>();
            for (int i = 0; i < resArray.Length; i++)
            {
                var v = new Vector2Int(resArray[i].width, resArray[i].height);
                if (!set.Contains(v)) { set.Add(v); _choices.Add(v); }
            }

            // Fallback si Unity renvoie vide (rare mais possible en mode fenêtré)
            if (_choices.Count == 0)
            {
                var di = Screen.mainWindowDisplayInfo;
                _choices.Add(new Vector2Int(di.width, di.height));
            }

            // Tri desc (plus grande → plus petite)
            _choices.Sort((a, b) =>
            {
                int byW = b.x.CompareTo(a.x);
                return (byW != 0) ? byW : b.y.CompareTo(a.y);
            });
        }

        private void FillDropdownLabels()
        {
            _resolutionDropdown.ClearOptions();
            var opts = new List<string>(_choices.Count);
            for (int i = 0; i < _choices.Count; i++)
            {
                var v = _choices[i];
                opts.Add($"{v.x} x {v.y}");
            }
            _resolutionDropdown.AddOptions(opts);
            _resolutionDropdown.RefreshShownValue();
        }

        private void SelectInDropdown(Vector2Int res)
        {
            int idx = _choices.FindIndex(v => v == res);
            if (idx < 0) idx = 0;

            _suppressDropdownCb = true;
            _resolutionDropdown.value = idx;
            _resolutionDropdown.RefreshShownValue();
            _suppressDropdownCb = false;
        }

        private Vector2Int GetSelectedResolutionFromUI()
        {
            int idx = Mathf.Clamp(_resolutionDropdown.value, 0, Mathf.Max(0, _choices.Count - 1));
            return _choices.Count > 0 ? _choices[idx] : CurrentResolution;
        }

        private Vector2Int? FindClosestOrExact(Vector2Int target)
        {
            int idx = _choices.FindIndex(v => v == target);
            if (idx >= 0) return _choices[idx];

            // Si pas d’exacte: choisir la plus grande <= target, sinon la plus grande dispo
            for (int i = 0; i < _choices.Count; i++)
            {
                var v = _choices[i];
                if (v.x <= target.x && v.y <= target.y)
                    return v;
            }
            return _choices.Count > 0 ? _choices[0] : (Vector2Int?)null;
        }

        private void OnDropdownChanged(int _)
        {
            if (_suppressDropdownCb) return;
            // Ici on ne fait rien d’autre : Apply/Revert gèrent les changements
            var sel = GetSelectedResolutionFromUI();
            Debug.Log($"[ResolutionController] UI selected → {sel.x}x{sel.y} (HasChanges={HasChanges()})");
        }

        // --- Clé stable d’écran (même principe que ton MonitorController) ---
        private struct DisplayKey
        {
            public readonly string name;
            public readonly RectInt workArea;

            public DisplayKey(string name, RectInt workArea)
            {
                this.name = name ?? "";
                this.workArea = workArea;
            }

            public bool Equals(DisplayKey other)
            {
                return name == other.name &&
                       workArea.x == other.workArea.x &&
                       workArea.y == other.workArea.y &&
                       workArea.width == other.workArea.width &&
                       workArea.height == other.workArea.height;
            }
            public override bool Equals(object obj) => obj is DisplayKey dk && Equals(dk);
            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = name.GetHashCode();
                    hash = (hash * 397) ^ workArea.x;
                    hash = (hash * 397) ^ workArea.y;
                    hash = (hash * 397) ^ workArea.width;
                    hash = (hash * 397) ^ workArea.height;
                    return hash;
                }
            }
        }

        private static DisplayKey MakeKey(DisplayInfo di) => new DisplayKey(di.name, di.workArea);
    }
}
