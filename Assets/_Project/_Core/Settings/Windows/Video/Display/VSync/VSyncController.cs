using UnityEngine;

namespace LightHouse.Core.Settings.Video.Display.VSync
{
    public class VSyncController : MonoBehaviour, IOption
    {
        [Header("UI")]
        public OptionToggle Toggle; 

        [Header("State (committed)")]
        public bool CurrentVSync;    // état committé (appliqué)
        public bool BackupVSync;     // miroir pour revert

        private bool _suppress;      // évite les boucles pendant MAJ UI

        private void Awake()
        {
            if (!Toggle)
            {
                Debug.LogError("[VSyncController] OptionToggle manquant.");
                enabled = false;
                return;
            }

            // Seed depuis le système
            CurrentVSync = QualitySettings.vSyncCount > 0;
            BackupVSync = CurrentVSync;

            _suppress = true;
            Toggle.SwitchSelected(CurrentVSync);
            _suppress = false;

            Toggle.OnValueChanged += OnToggleChanged;
        }

        private void OnDestroy()
        {
            if (Toggle) Toggle.OnValueChanged -= OnToggleChanged;
        }

        public void OnFrameRateDifferentThanIllimited()
        {
            if (QualitySettings.vSyncCount > 0) QualitySettings.vSyncCount = 0;

            // ✅ on commit l’état pour rester cohérent (sinon HasChanges peut rester faux/ambigu)
            CurrentVSync = false;
            BackupVSync = false;

            // ✅ UI = OFF + désactivation
            _suppress = true;
            Toggle.SetValueWithoutNotify(false);
            Toggle.SwitchSelected(false);
            Toggle.DisableAll();
            _suppress = false;
        }

        public void OnFrameRateIllimited()
        {
            // ✅ on redonne la main à l’utilisateur sans forcer ON/OFF
            Toggle.EnableAll();
            QualitySettings.vSyncCount = CurrentVSync ? 1 : 0;

            _suppress = true;
            Toggle.SetValueWithoutNotify(CurrentVSync);
            Toggle.SwitchSelected(CurrentVSync);
            _suppress = false;
        }




        private void OnToggleChanged(bool value)
        {
            if (_suppress) return;
            // Pas d’auto-apply : tes boutons Apply/Revert s’en occupent
            // (Si tu veux auto-apply, appelle Apply() ici.)
            Debug.Log($"[VSyncController] UI changed → {(value ? "ON" : "OFF")} (HasChanges={HasChanges()})");
        }

        // ---------- IOption ----------

        public bool HasChanges()
        {
            if (!Toggle) return false;
            return Toggle.isOn != CurrentVSync;
        }

        public void Apply()
        {
            if (!Toggle) return;
            if (!HasChanges()) return;                // ✅ utilise HasChanges ici

            bool desired = Toggle.isOn;

            QualitySettings.vSyncCount = desired ? 1 : 0;

            // Optionnel : si tu veux libérer le cap quand VSync ON, garde ceci :
            if (desired && Application.targetFrameRate != -1)
                Application.targetFrameRate = -1;

            CurrentVSync = BackupVSync = desired;

            _suppress = true;
            Toggle.SetValueWithoutNotify(CurrentVSync);
            Toggle.SwitchSelected(CurrentVSync);
            _suppress = false;

            Debug.Log($"[VSyncController] Apply → vSync={(desired ? "ON(1)" : "OFF(0)")}, targetFPS={Application.targetFrameRate}");
        }

        public void Revert()
        {
            // Revient à l’état committé précédent (backup)
            QualitySettings.vSyncCount = BackupVSync ? 1 : 0;

            if (BackupVSync && Application.targetFrameRate != -1)
                Application.targetFrameRate = -1;

            CurrentVSync = BackupVSync;

            _suppress = true;
            Toggle.SwitchSelected(CurrentVSync);
            _suppress = false;

            Debug.Log($"[VSyncController] Revert → vSync={(CurrentVSync ? "ON(1)" : "OFF(0)")}, targetFPS={Application.targetFrameRate}");
        }
    }
}
