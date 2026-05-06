using System;
using UnityEngine;

namespace LightHouse.Core.Settings.Video.Graphism.Quality
{

    public static class GraphismHandlerData
    {
        public static bool IsInitialized { get; private set; }
        public static GfxTier CurrentTier { get; private set; } = GfxTier.High;
        public static VideoPresetSettings CurrentPreset { get; private set; }

        // Optionnel : event si d’autres systèmes veulent réagir
        public static event Action<GfxTier, VideoPresetSettings> OnPresetApplied;

        public static void SetPreset(VideoPresetSettings preset, GfxTier tier)
        {
            if (preset == null)
            {
                Debug.LogWarning("[GraphismHandlerData] SetPreset appelé avec null.");
                return;
            }

            // 1) Quality Level (si tu stockes l'index dans le SO)
            if (preset.QualityIndex >= 0 && preset.QualityIndex < QualitySettings.names.Length)
            {
                QualitySettings.SetQualityLevel(preset.QualityIndex, true);
            }

            // 2) Appliquer tes autres champs (ex: bools/numériques) sur tes systèmes :
            // preset.ApplyToRuntime(); // à implémenter selon tes besoins (HDRP, post-process, etc.)

            CurrentTier = tier;
            IsInitialized = true;
            CurrentPreset = preset;

            OnPresetApplied?.Invoke(tier, preset);
            //Debug.Log($"[GraphismHandlerData] Preset '{preset.name}' appliqué → Tier={tier} | Quality={QualitySettings.names[QualitySettings.GetQualityLevel()]}");
        }
    }
}