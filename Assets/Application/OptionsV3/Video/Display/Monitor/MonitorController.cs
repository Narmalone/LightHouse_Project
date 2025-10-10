using System.Collections.Generic;
using UnityEngine;

namespace LightHouse.Options.V3
{
    public class MonitorController : MonoBehaviour, IOption
    {
        [Header("UI (passive)")]
        public OptionEnumPassive optionEnum;
        public SimpleDisplayModeTogglerWithUI modeProvider; // assigne dans l’inspector

        public bool ShowDebugMode = true;

        [Header("Keys")]
        public KeyCode nextKey = KeyCode.LeftArrow;
        public KeyCode prevKey = KeyCode.RightArrow;
        public KeyCode applyKey = KeyCode.UpArrow;
        public KeyCode revertKey = KeyCode.DownArrow;

        // ---- Etat interne ----
        private bool _opInFlight;

        private List<DisplayInfo> _infos = new(); // snapshot courant (index UI)
        private List<DisplayKey> _keys = new();  // clés stables alignées sur _infos

        private DisplayKey _currentKey;           // où se trouve réellement la fenêtre (état courant live)
        private DisplayKey _backupKey;            // ✅ état "committé" (ce vers quoi Revert revient)
        private int _selectedIndex;               // curseur UI (index -> key via _keys)

        private Vector2Int _backupResolution;     // ✅ résolution committée
        private FullScreenMode _backupMode;       // ✅ mode committé

        private void Awake()
        {
            if (!optionEnum)
            {
                Debug.LogError("[MonitorController] optionEnum (passive) manquant.");
                enabled = false;
                return;
            }
            optionEnum.OnPrevClicked += SelectPrev;
            optionEnum.OnNextClicked += SelectNext;
        }

        private void OnEnable()
        {
            InitializeUI();
            if (modeProvider) modeProvider.OnModeCommitted += OnModeCommitted;
        }

        private void OnModeCommitted(FullScreenMode m)
        {
            // Si tu veux refléter quelque chose dans le debug/backup
            _backupMode = m; // garde la notion de "dernier mode validé"
        }


        private void OnDisable()
        {
            if (modeProvider) modeProvider.OnModeCommitted -= OnModeCommitted;

            if (optionEnum)
            {
                optionEnum.OnPrevClicked -= SelectPrev;
                optionEnum.OnNextClicked -= SelectNext;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(nextKey)) SelectNext();
            if (Input.GetKeyDown(prevKey)) SelectPrev();
            if (Input.GetKeyDown(applyKey)) Apply();
            if (Input.GetKeyDown(revertKey)) Revert();
        }

        // ---------- Init & mapping ----------

        private void InitializeUI()
        {
            RefreshInfos(); // remplit _infos + _keys

            var labels = new List<string>();
            if (_infos.Count == 0)
            {
                labels.Add($"Display 1 ({Screen.currentResolution.width}x{Screen.currentResolution.height})");
            }
            else
            {
                for (int i = 0; i < _infos.Count; i++)
                {
                    var di = _infos[i];
                    labels.Add($"Display {i + 1}  {di.name}  [{di.width}x{di.height}]  area({di.workArea.x},{di.workArea.y},{di.workArea.width},{di.workArea.height})");
                }
            }
            optionEnum.SetChoices(labels);

            // current depuis la fenêtre système → clé stable
            var cur = Screen.mainWindowDisplayInfo;
            _currentKey = MakeKey(cur);

            // ✅ au lancement, le backup = current (état committé initial)
            _backupKey = _currentKey;
            _backupResolution = new Vector2Int(Screen.currentResolution.width, Screen.currentResolution.height);
            _backupMode = Screen.fullScreenMode;

            _selectedIndex = IndexFromKey(_currentKey);
            if (_selectedIndex < 0) _selectedIndex = 0;
            optionEnum.ShowIndex(_selectedIndex);
        }

        private void RefreshInfos()
        {
            _infos.Clear();
            Screen.GetDisplayLayout(_infos);

            _keys.Clear();
            for (int i = 0; i < _infos.Count; i++)
                _keys.Add(MakeKey(_infos[i]));
        }

        private static DisplayKey MakeKey(DisplayInfo di)
            => new DisplayKey(di.name, di.workArea);

        private int IndexFromKey(DisplayKey key)
        {
            for (int i = 0; i < _keys.Count; i++)
                if (_keys[i].Equals(key)) return i;
            // fallback souple: nom identique si workArea a légèrement varié
            for (int i = 0; i < _keys.Count; i++)
                if (_keys[i].name == key.name) return i;
            return -1;
        }

        // ---------- Navigation ----------

        private void SelectNext()
        {
            if (_opInFlight) return;
            int count = Mathf.Max(1, optionEnum.Count);
            _selectedIndex = (_selectedIndex + 1) % count;
            optionEnum.ShowIndex(_selectedIndex);
        }

        private void SelectPrev()
        {
            if (_opInFlight) return;
            int count = Mathf.Max(1, optionEnum.Count);
            _selectedIndex = (_selectedIndex - 1 + count) % count;
            optionEnum.ShowIndex(_selectedIndex);
        }

        // ---------- IOption ----------

        public bool HasChanges()
        {
            if (_keys.Count == 0) return false;
            var selKey = _keys[Mathf.Clamp(_selectedIndex, 0, _keys.Count - 1)];
            // ✅ comparaison sélection vs état COMMITTÉ (backup) ou vs courant ?
            // Ici on garde la logique "diff vs courant live" :
            return !selKey.Equals(_currentKey);
        }

        public void Apply()
        {
            if (_opInFlight) return;
            if (!HasChanges()) return;

            RefreshInfos();
            if (_infos.Count == 0) return;

            _selectedIndex = Mathf.Clamp(_selectedIndex, 0, _infos.Count - 1);
            var target = _infos[_selectedIndex];

            // On prend le mode "committé" du DisplayModeController si dispo, sinon l’état système
            var desiredMode = modeProvider ? modeProvider.CurrentCommittedMode : Screen.fullScreenMode;

            _opInFlight = true;
            StartCoroutine(MoveAndApplyMode(target, desiredMode));
        }

        private System.Collections.IEnumerator MoveAndApplyMode(DisplayInfo target, FullScreenMode desiredMode)
        {
            // capture la résolution live
            int liveW = Screen.currentResolution.width;
            int liveH = Screen.currentResolution.height;

            // si on est en exclusive ET qu’on va bouger → passer en FSW pour autoriser le move proprement
            bool isExclusiveNow = (Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen);
            if (isExclusiveNow)
            {
                Screen.SetResolution(liveW, liveH, FullScreenMode.FullScreenWindow);
                // attendre que Unity bascule vraiment (évite l’OS qui recolle exclusive en douce)
                yield return null; // 1 frame
                float t = 0f;
                while (Screen.fullScreenMode != FullScreenMode.FullScreenWindow && (t += Time.unscaledDeltaTime) < 1.0f)
                    yield return null;
            }

            // clamp résol cible au display
            int targetW = Mathf.Min(liveW, target.width);
            int targetH = Mathf.Min(liveH, target.height);

            // move
            var op = Screen.MoveMainWindowTo(in target, Vector2Int.zero);
            // attendre la fin du move
            while (!op.isDone) yield return null;
            yield return null; // 1 frame pour stabiliser la nouvelle surface

            // appliquer le MODE FINAL demandé (une seule fois)
            Screen.SetResolution(targetW, targetH, desiredMode);
            yield return null;
            // Sur certaines configs, attendre que le mode soit effectif
            float t2 = 0f;
            while (Screen.fullScreenMode != desiredMode && (t2 += Time.unscaledDeltaTime) < 1.0f)
                yield return null;

            // MAJ états internes + backup
            _currentKey = MakeKey(Screen.mainWindowDisplayInfo);
            _backupKey = _currentKey;
            _backupResolution = new Vector2Int(targetW, targetH);
            _backupMode = desiredMode;

            RefreshInfos();
            _selectedIndex = IndexFromKey(_currentKey);
            if (_selectedIndex < 0) _selectedIndex = 0;
            optionEnum.ShowIndex(_selectedIndex);

            _opInFlight = false;
            Debug.Log($"[MonitorController] Apply → '{_currentKey.name}' {targetW}x{targetH} mode:{desiredMode}");
        }

        public void Revert()
        {
            if (_opInFlight) return;
            if (_infos.Count == 0) RefreshInfos();
            if (_infos.Count == 0) return;

            // 1) "UI-only revert" : si la sélection UI ne reflète pas l'état réel, on snap l'UI sur le courant.
            if (_keys.Count > 0)
            {
                _selectedIndex = Mathf.Clamp(_selectedIndex, 0, _keys.Count - 1);
                var selKey = _keys[_selectedIndex];
                if (!selKey.Equals(_currentKey))
                {
                    // Snap UI sur l'état réel sans rien appliquer au système
                    int curIdx = IndexFromKey(_currentKey);
                    if (curIdx < 0) curIdx = 0;
                    _selectedIndex = curIdx;
                    optionEnum.ShowIndex(_selectedIndex);
                    Debug.Log("[MonitorController] Revert(UI) → réalignement de l'UI sur l'écran courant, aucun changement système.");
                    return;
                }
            }

            // 2) Si l'état courant est déjà l'état committé → rien à faire
            if (_currentKey.Equals(_backupKey)) return;

            // 3) "System revert" : on revient au committed (écran + res + mode)
            _opInFlight = true;

            int backIdx = IndexFromKey(_backupKey);
            if (backIdx < 0)
            {
                _opInFlight = false;
                Debug.LogWarning("[MonitorController] Revert: display backup introuvable.");
                return;
            }

            var back = _infos[backIdx];

            var committedW = _backupResolution.x;
            var committedH = _backupResolution.y;
            var committedM = _backupMode;

            var op = Screen.MoveMainWindowTo(in back, Vector2Int.zero);
            op.completed += _ =>
            {
                Screen.SetResolution(committedW, committedH, committedM);

                _currentKey = MakeKey(Screen.mainWindowDisplayInfo);

                // Après revert, on reste committé sur ces valeurs
                _backupKey = _currentKey;
                _backupResolution = new Vector2Int(committedW, committedH);
                _backupMode = committedM;

                RefreshInfos();
                _selectedIndex = IndexFromKey(_currentKey);
                if (_selectedIndex < 0) _selectedIndex = 0;
                optionEnum.ShowIndex(_selectedIndex);

                _opInFlight = false;
                Debug.Log($"[MonitorController] Revert(System) → '{_currentKey.name}' area{_currentKey.workArea}  res:{committedW}x{committedH} mode:{committedM}");
            };
        }


        // ---------- Debug GUI ----------
        private void OnGUI()
        {
            if(!ShowDebugMode) return;
            int x = 10, y = 10, w = 1100, h = 20, dy = 18;
            var curDi = Screen.mainWindowDisplayInfo;
            var curKeyNow = MakeKey(curDi);

            GUI.Label(new Rect(x, y, w, h), "[MonitorController]");
            y += dy;
            GUI.Label(new Rect(x, y, w, h), $"opInFlight:    {_opInFlight}");
            y += dy;
            GUI.Label(new Rect(x, y, w, h), $"currentKey:    '{_currentKey.name}' area{_currentKey.workArea}   (system now: '{curKeyNow.name}' area{curKeyNow.workArea})");
            y += dy;
            GUI.Label(new Rect(x, y, w, h), $"backupKey:     '{_backupKey.name}' area{_backupKey.workArea}");
            y += dy;
            GUI.Label(new Rect(x, y, w, h), $"selectedIndex: {_selectedIndex}/{_keys.Count - 1}");
            y += dy;
            GUI.Label(new Rect(x, y, w, h), $"committed res/mode: {_backupResolution.x}x{_backupResolution.y} | {_backupMode}");
            y += dy;
            GUI.Label(new Rect(x, y, w, h), $"Displays count: {_infos.Count}");
            y += dy;

            for (int i = 0; i < _infos.Count; i++)
            {
                var di = _infos[i];
                var k = _keys[i];
                string tags = "";
                if (k.Equals(_currentKey)) tags += " <current>";
                if (k.Equals(_backupKey)) tags += " <committed>";
                if (i == _selectedIndex) tags += " <UI selected>";
                GUI.Label(new Rect(x, y, w, h),
                    $"[{i}] '{di.name}' size:{di.width}x{di.height} workArea({di.workArea.x},{di.workArea.y},{di.workArea.width},{di.workArea.height}){tags}");
                y += dy;
            }
        }

        // ---------- Clé stable ----------
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

            public override string ToString() => $"'{name}' area({workArea.x},{workArea.y},{workArea.width},{workArea.height})";
        }
    }
}
