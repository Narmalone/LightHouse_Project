using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace LightHouse.Core.Settings.Video.Display.FrameRate
{
    public class FrameRateController : MonoBehaviour, IOption
    {
        [Header("UI")]
        public TMP_Dropdown _frameRateDropDown;

        [Header("State (committed)")]
        public int CurrentFrameRate;   // -1 = illimité
        public int BackupFrameRate;

        // internes
        private readonly List<int> _choices = new() { -1, 30, 60, 75, 90, 120, 144, 165, 240, 360 };
        private bool _suppressDropdown;

        public event Action<int> OnApplied;

        private void Awake()
        {
            if (!_frameRateDropDown)
            {
                Debug.LogError("[FrameRateController] TMP_Dropdown manquant.");
                enabled = false;
                return;
            }
        }

        private void OnEnable()
        {
            _frameRateDropDown.onValueChanged.AddListener(OnDropdownChanged);
            BuildDropdown();
            SeedFromSystem();
        }

        private void OnDisable()
        {
            _frameRateDropDown.onValueChanged.RemoveListener(OnDropdownChanged);
        }

        // --- IOption ---

        public bool HasChanges()
        {
            int desired = GetSelectedValue();
            return desired != CurrentFrameRate;
        }

        public void Apply()
        {
            int desired = GetSelectedValue();
            if (!HasChanges()) return;

            // Pour que Application.targetFrameRate soit respecté, on coupe la VSync ici.
            if (desired > 0) QualitySettings.vSyncCount = 0; //On force le coupage de la vsync en amont

            Application.targetFrameRate = desired; // -1 = illimité
            CurrentFrameRate = desired;
            BackupFrameRate = CurrentFrameRate;


            // réaligne l’UI au committé (utile si valeurs clampées plus tard)
            SelectInDropdown(CurrentFrameRate);
            OnApplied?.Invoke(CurrentFrameRate);
            Debug.Log($"[FrameRateController] Apply → {(desired < 0 ? "Illimité" : desired + " FPS")}");
        }

        public void Revert()
        {
            // Revenir au dernier committé (ne touche pas à la VSync ici)
            Application.targetFrameRate = BackupFrameRate;
            CurrentFrameRate = BackupFrameRate;
            SelectInDropdown(CurrentFrameRate);

            Debug.Log($"[FrameRateController] Revert → {(CurrentFrameRate < 0 ? "Illimité" : CurrentFrameRate + " FPS")}");
        }

        // --- UI helpers ---

        private void BuildDropdown()
        {
            var labels = new List<string>(_choices.Count);
            for (int i = 0; i < _choices.Count; i++)
                labels.Add(_choices[i] < 0 ? "Illimité" : _choices[i] + " FPS");

            _frameRateDropDown.ClearOptions();
            _frameRateDropDown.AddOptions(labels);
            _frameRateDropDown.RefreshShownValue();
        }

        private void SeedFromSystem()
        {
            // Application.targetFrameRate <= 0 signifie "illimité"
            int sys = Application.targetFrameRate <= 0 ? -1 : Application.targetFrameRate;

            // trouve la valeur exacte ou, à défaut, la plus proche en-dessous; sinon la plus grande dispo (illimité)
            int toSelect = FindClosestOrExact(sys);
            CurrentFrameRate = toSelect;
            BackupFrameRate = CurrentFrameRate;

            SelectInDropdown(CurrentFrameRate);
        }

        private void SelectInDropdown(int value)
        {
            int idx = _choices.FindIndex(v => v == value);
            if (idx < 0) idx = 0;

            _suppressDropdown = true;
            _frameRateDropDown.value = idx;
            _frameRateDropDown.RefreshShownValue();
            _suppressDropdown = false;
        }

        private int GetSelectedValue()
        {
            int idx = Mathf.Clamp(_frameRateDropDown.value, 0, _choices.Count - 1);
            return _choices[idx];
        }

        private int FindClosestOrExact(int target)
        {
            // exact
            int idx = _choices.FindIndex(v => v == target);
            if (idx >= 0) return _choices[idx];

            // si target < 0 → illimité
            if (target < 0) return -1;

            // sinon on prend la plus grande valeur <= target, sinon la plus grande dispo (illimité)
            for (int i = _choices.Count - 1; i >= 0; i--)
            {
                int v = _choices[i];
                if (v < 0) continue; // saute "illimité" dans ce sens
                if (v <= target) return v;
            }
            return -1;
        }

        private void OnDropdownChanged(int _)
        {
            if (_suppressDropdown) return;
            var sel = GetSelectedValue();
            Debug.Log($"[FrameRateController] UI selected → {(sel < 0 ? "Illimité" : sel + " FPS")} (HasChanges={HasChanges()})");
            // Pas d'auto-apply ici — laisse les boutons Apply/Revert gérer.
        }

        // (Optionnel) Petit overlay debug :
        private void OnGUI()
        {
            /*const int w = 500;
            GUI.Label(new Rect(10, 10, w, 20), "[FrameRateController]");
            GUI.Label(new Rect(10, 30, w, 20), $"Committed: {(CurrentFrameRate < 0 ? "Illimité" : CurrentFrameRate + " FPS")}");
            GUI.Label(new Rect(10, 50, w, 20), $"Selected:  {(_choices[Mathf.Clamp(_frameRateDropDown.value, 0, _choices.Count - 1)] < 0 ? "Illimité" : _choices[_frameRateDropDown.value] + " FPS")}");
            GUI.Label(new Rect(10, 70, w, 20), $"System:    {(Application.targetFrameRate <= 0 ? "Illimité" : Application.targetFrameRate + " FPS")} | vSync:{QualitySettings.vSyncCount}");*/
        }
    }
}
