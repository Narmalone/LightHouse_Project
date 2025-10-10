using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace LightHouse.Options.V3
{
    public class RefreshRateController : MonoBehaviour, IOption
    {
        [Header("UI")]
        public TMP_Dropdown _resolutionDropdown; // dropdown des Hz

        [Header("State (committed)")]
        public RefreshRate CurrentRefreshRate; // committé (struct)
        public RefreshRate BackupRefreshRate;  // miroir pour Revert

        // --- internes ---
        private readonly List<int> _choicesHz = new(); // triés desc
        private bool _suppressDropdownCb;

        // détection de changement d'écran + résolution système
        private DisplayKey _displayKey;
        private Vector2Int _resSnapshot;

        private void Awake()
        {
            if (!_resolutionDropdown)
            {
                Debug.LogError("[RefreshRateController] TMP_Dropdown manquant.");
                enabled = false;
                return;
            }
        }

        private void OnEnable()
        {
            _resolutionDropdown.onValueChanged.AddListener(OnDropdownChanged);
            RebuildForCurrentMonitor(selectHighest: true);
        }

        private void OnDisable()
        {
            _resolutionDropdown.onValueChanged.RemoveListener(OnDropdownChanged);
        }

        private void Update()
        {
            var keyNow = MakeKey(Screen.mainWindowDisplayInfo);
            var sysRes = Screen.currentResolution;
            var resNow = new Vector2Int(sysRes.width, sysRes.height);

            if (!_displayKey.Equals(keyNow) || _resSnapshot != resNow)
            {
                _displayKey = keyNow;
                _resSnapshot = resNow;
                RebuildForCurrentMonitor(selectHighest: true); // reprend le plus élevé
            }
        }

        // --- IOption ---

        public bool HasChanges()
        {
            int selHz = GetSelectedHzFromUI();
            return selHz != Hz(CurrentRefreshRate);
        }

        public void Apply()
        {
            int selHz = GetSelectedHzFromUI();
            if (!HasChanges()) return;

            var mode = Screen.fullScreenMode;
            var cr = Screen.currentResolution;

            Screen.SetResolution(cr.width, cr.height, mode, ToRefreshRate(selHz));

            // Commit
            CurrentRefreshRate = ToRefreshRate(selHz);
            BackupRefreshRate = CurrentRefreshRate;

            SelectInDropdown(selHz);
            Debug.Log($"[RefreshRateController] Apply → {cr.width}x{cr.height} @ {selHz} Hz | mode:{mode}");
        }

        public void Revert()
        {
            int hz = Hz(BackupRefreshRate);

            var mode = Screen.fullScreenMode;
            var cr = Screen.currentResolution;

            Screen.SetResolution(cr.width, cr.height, mode, ToRefreshRate(hz));

            CurrentRefreshRate = BackupRefreshRate;
            SelectInDropdown(hz);

            Debug.Log($"[RefreshRateController] Revert → {cr.width}x{cr.height} @ {hz} Hz | mode:{mode}");
        }

        // --- Public helper (appelable après un move d’écran) ---
        public void RebuildForCurrentMonitor(bool selectHighest = true)
        {
            BuildChoicesForCurrentMonitor();

            if (_choicesHz.Count == 0)
            {
                int sysHz = SafeGetHz(Screen.currentResolution);
                CurrentRefreshRate = ToRefreshRate(sysHz);
                BackupRefreshRate = CurrentRefreshRate;

                _resolutionDropdown.ClearOptions();
                _resolutionDropdown.AddOptions(new List<string> { $"{sysHz} Hz" });
                _resolutionDropdown.value = 0;
                _resolutionDropdown.RefreshShownValue();
                return;
            }

            int sysHzNow = SafeGetHz(Screen.currentResolution);
            int toSelectHz = selectHighest ? _choicesHz[0] : FindClosestOrExactHz(sysHzNow);

            CurrentRefreshRate = ToRefreshRate(toSelectHz);
            BackupRefreshRate = CurrentRefreshRate;

            FillDropdownLabels();
            SelectInDropdown(toSelectHz);

            Debug.Log($"[RefreshRateController] RebuildForCurrentMonitor → {toSelectHz} Hz (highest={selectHighest})");
        }

        // --- Internes: build / UI ---

        private void BuildChoicesForCurrentMonitor()
        {
            _choicesHz.Clear();

            _displayKey = MakeKey(Screen.mainWindowDisplayInfo);
            var cr = Screen.currentResolution;
            _resSnapshot = new Vector2Int(cr.width, cr.height);

            var resArray = Screen.resolutions;
            var set = new HashSet<int>();
            for (int i = 0; i < resArray.Length; i++)
            {
                int hz = SafeGetHz(resArray[i]);
                if (hz <= 0) continue;
                if (set.Add(hz)) _choicesHz.Add(hz);
            }

            if (_choicesHz.Count == 0)
            {
                _choicesHz.Add(SafeGetHz(cr));
            }

            _choicesHz.Sort((a, b) => b.CompareTo(a)); // desc
        }

        private void FillDropdownLabels()
        {
            _resolutionDropdown.ClearOptions();
            var opts = new List<string>(_choicesHz.Count);
            for (int i = 0; i < _choicesHz.Count; i++)
                opts.Add($"{_choicesHz[i]} Hz");
            _resolutionDropdown.AddOptions(opts);
            _resolutionDropdown.RefreshShownValue();
        }

        private void SelectInDropdown(int hz)
        {
            int idx = _choicesHz.FindIndex(v => v == hz);
            if (idx < 0) idx = 0;

            _suppressDropdownCb = true;
            _resolutionDropdown.value = idx;
            _resolutionDropdown.RefreshShownValue();
            _suppressDropdownCb = false;
        }

        private int GetSelectedHzFromUI()
        {
            int idx = Mathf.Clamp(_resolutionDropdown.value, 0, Mathf.Max(0, _choicesHz.Count - 1));
            return _choicesHz.Count > 0 ? _choicesHz[idx] : Hz(CurrentRefreshRate);
        }

        private int FindClosestOrExactHz(int targetHz)
        {
            int idx = _choicesHz.FindIndex(v => v == targetHz);
            if (idx >= 0) return _choicesHz[idx];

            for (int i = 0; i < _choicesHz.Count; i++)
                if (_choicesHz[i] <= targetHz) return _choicesHz[i];

            return _choicesHz[0];
        }

        // ✅ Callback UI
        private void OnDropdownChanged(int _)
        {
            if (_suppressDropdownCb) return;
            var selHz = GetSelectedHzFromUI();
            Debug.Log($"[RefreshRateController] UI selected → {selHz} Hz (HasChanges={HasChanges()})");
            // Pas d'auto-apply ici
        }

        // --- Utils Hz ---
        private static int Hz(RefreshRate rr)
        {
            // rr.value est un double, on convertit en int Hz
            return Mathf.RoundToInt((float)rr.value);
        }

        private static int SafeGetHz(Resolution r)
        {
            return Mathf.RoundToInt((float)r.refreshRateRatio.value);
        }

        private static RefreshRate ToRefreshRate(int hz)
        {
            return new RefreshRate { numerator = (uint)hz, denominator = 1 };
        }

        // --- Clé stable d’écran (comme MonitorController) ---
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
