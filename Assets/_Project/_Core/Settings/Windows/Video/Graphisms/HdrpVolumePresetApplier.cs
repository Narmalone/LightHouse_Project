using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace LightHouse.Core.Settings.Video.Graphism.Quality
{
    public enum GfxTier { Low, Medium, High, Ultra }

    public class HdrpVolumePresetApplier : MonoBehaviour
    {
        [Header("Scene Global Volume")]
        [SerializeField] private Volume globalVolume;

        [Header("Quality Levels (Project Settings → Quality)")]
        [Tooltip("Index du Quality Level à utiliser pour chaque GfxTier (ordre et indices doivent correspondre à tes Quality Levels).")]
        [SerializeField] private int[] qualityIndexByTier = { 0, 1, 2, 3 }; // Low, Med, High, Ultra

        public int currentTier = -1;

        void Start()
        {
            if (!globalVolume)
                Debug.LogWarning("[HDRP Preset] Global Volume non assigné.");
            if (GraphismHandlerData.CurrentPreset != null) Apply(GraphismHandlerData.CurrentTier);

            GraphismHandlerData.OnPresetApplied += GraphismHandlerData_OnPresetChanged;
        }

        private void OnDestroy()
        {
            GraphismHandlerData.OnPresetApplied -= GraphismHandlerData_OnPresetChanged;
        }

        private void GraphismHandlerData_OnPresetChanged(GfxTier tier, VideoPresetSettings obj)
        {
            Apply(obj.Tier);
            Debug.Log("preset applied: " + tier);
        }

        // ---------- Helpers ----------
        bool Get<T>(out T c) where T : VolumeComponent
        {
            c = null;
            return globalVolume && globalVolume.profile && globalVolume.profile.TryGet(out c);
        }

        void SwitchQualityLevel(GfxTier tier)
        {
            int idx = (int)tier;
            if (qualityIndexByTier != null && idx < qualityIndexByTier.Length)
            {
                int q = Mathf.Clamp(qualityIndexByTier[idx], 0, QualitySettings.names.Length - 1);
                QualitySettings.SetQualityLevel(q, true); // active le HDRP Asset mappé à ce Quality Level
            }
        }

        // ---------- API principale ----------
        public void Apply(GfxTier tier)
        {
            currentTier = (int)tier;

            // 1) Change Quality Level (→ change d'HDRP Asset)
            SwitchQualityLevel(tier);

            // 2) (Optionnel) Change de Volume Profile complet
            //SwitchVolumeProfile(tier);

            // 3) Ajuste les overrides du profile courant
            ApplyFog(tier);
            ApplyVolumetricClouds(tier);
            ApplySky(tier);
            ApplySSGI(tier);
            ApplySSR(tier);
            ApplyHDShadowSettings(tier);
            ApplyMicroShadowing(tier);
            ApplyContactShadows(tier);
            ApplyExposure(tier);

            //Debug.Log($"[HDRP Preset] {tier} appliqué | Quality={QualitySettings.names[QualitySettings.GetQualityLevel()]} | Profile={globalVolume?.profile?.name}");
        }

        // ---------- Overrides par effet ----------
        void ApplyFog(GfxTier tier)
        {
            if (Get<Fog>(out var fog))
            {
                fog.active = true;
                fog.enabled.Override(true);

                switch (tier)
                {
                    case GfxTier.Low:
                        fog.enableVolumetricFog.Override(false);   // pas de volumétrique
                        fog.meanFreePath.Override(60f);             // plus dense
                        break;
                    case GfxTier.Medium:
                        fog.enableVolumetricFog.Override(true);
                        fog.meanFreePath.Override(100f);
                        break;
                    case GfxTier.High:
                        fog.enableVolumetricFog.Override(true);
                        fog.meanFreePath.Override(160f);
                        break;
                    case GfxTier.Ultra:
                        fog.enableVolumetricFog.Override(true);
                        fog.meanFreePath.Override(220f);
                        break;
                }
            }
        }

        void ApplyVolumetricClouds(GfxTier tier)
        {
            if (Get<VolumetricClouds>(out var c))
            {
                switch (tier)
                {
                    case GfxTier.Low:
                        c.active = false;
                        break;
                    case GfxTier.Medium:
                        c.active = true;
                        c.numPrimarySteps.Override(32);
                        c.numLightSteps.Override(4);
                        c.shadowResolution.Override(VolumetricClouds.CloudShadowResolution.Low128);
                        break;
                    case GfxTier.High:
                        c.active = true;
                        c.numPrimarySteps.Override(48);
                        c.numLightSteps.Override(6);
                        c.shadowResolution.Override(VolumetricClouds.CloudShadowResolution.Medium256);
                        break;
                    case GfxTier.Ultra:
                        c.active = true;
                        c.numPrimarySteps.Override(64);
                        c.numLightSteps.Override(8);
                        c.shadowResolution.Override(VolumetricClouds.CloudShadowResolution.High512);
                        break;
                }
            }
        }

        void ApplySky(GfxTier tier)
        {
            if (Get<PhysicallyBasedSky>(out var sky))
                sky.active = true;

            if (Get<VisualEnvironment>(out var ve))
                ve.active = true;
        }

        void ApplySSGI(GfxTier tier)
        {
            // Nom de composant HDRP: ScreenSpaceGlobalIllumination
            if (Get<GlobalIllumination>(out var ssgi))
            {
                switch (tier)
                {
                    case GfxTier.Low:
                        ssgi.active = false;
                        break;
                    case GfxTier.Medium:
                        ssgi.active = true;
                        ssgi.quality.Override(0); // Low
                        ssgi.fullResolution = false;
                        ssgi.denoise = true;
                        break;
                    case GfxTier.High:
                        ssgi.active = true;
                        ssgi.quality.Override(1); // Medium
                        ssgi.fullResolution = false;
                        ssgi.denoise = true;
                        break;
                    case GfxTier.Ultra:
                        ssgi.active = true;
                        ssgi.quality.Override(2); // High
                        ssgi.fullResolution = true;
                        ssgi.denoise = true;
                        break;
                }
            }
        }

        void ApplySSR(GfxTier tier)
        {
            if (Get<ScreenSpaceReflection>(out var ssr))
            {
                switch (tier)
                {
                    case GfxTier.Low:
                        ssr.active = false;
                        break;
                    case GfxTier.Medium:
                        ssr.active = true;
                        ssr.quality.Override(0);         // Low
                        ssr.fullResolution = false;
                        ssr.depthBufferThickness.Override(0.2f);
                        break;
                    case GfxTier.High:
                        ssr.active = true;
                        ssr.quality.Override(1);         // Medium
                        ssr.fullResolution = false;
                        ssr.depthBufferThickness.Override(0.25f);
                        break;
                    case GfxTier.Ultra:
                        ssr.active = true;
                        ssr.quality.Override(2);         // High
                        ssr.fullResolution = true;
                        ssr.depthBufferThickness.Override(0.3f);
                        break;
                }
            }
        }

        void ApplyHDShadowSettings(GfxTier tier)
        {
            if (Get<HDShadowSettings>(out var sh))
            {
                sh.active = true;
                switch (tier)
                {
                    case GfxTier.Low:
                        sh.maxShadowDistance.Override(40f);
                        sh.cascadeShadowSplitCount.Override(1);
                        break;
                    case GfxTier.Medium:
                        sh.maxShadowDistance.Override(80f);
                        sh.cascadeShadowSplitCount.Override(2);
                        break;
                    case GfxTier.High:
                        sh.maxShadowDistance.Override(120f);
                        sh.cascadeShadowSplitCount.Override(2);
                        break;
                    case GfxTier.Ultra:
                        sh.maxShadowDistance.Override(160f);
                        sh.cascadeShadowSplitCount.Override(4);
                        break;
                }
            }
        }

        void ApplyMicroShadowing(GfxTier tier)
        {
            if (Get<MicroShadowing>(out var micro))
            {
                switch (tier)
                {
                    case GfxTier.Low:
                    case GfxTier.Medium:
                        micro.active = false;
                        break;
                    case GfxTier.High:
                    case GfxTier.Ultra:
                        micro.active = true;
                        micro.opacity.Override(0.5f);
                        break;
                }
            }
        }

        void ApplyContactShadows(GfxTier tier)
        {
            if (Get<ContactShadows>(out var cs))
            {
                switch (tier)
                {
                    case GfxTier.Low:
                        cs.active = false;
                        break;
                    case GfxTier.Medium:
                        cs.active = true;
                        cs.opacity.Override(0.5f);
                        cs.length.Override(8f);
                        cs.maxDistance.Override(20f);
                        cs.sampleCount = 8;
                        break;
                    case GfxTier.High:
                        cs.active = true;
                        cs.opacity.Override(0.7f);
                        cs.length.Override(12f);
                        cs.maxDistance.Override(30f);
                        cs.sampleCount = 16;
                        break;
                    case GfxTier.Ultra:
                        cs.active = true;
                        cs.opacity.Override(0.8f);
                        cs.length.Override(16f);
                        cs.maxDistance.Override(50f);
                        cs.sampleCount = 24;
                        break;
                }
            }
        }

        void ApplyExposure(GfxTier tier)
        {
            if (Get<Exposure>(out var exp))
            {
                exp.active = true;
                exp.adaptationMode.Override(AdaptationMode.Progressive);
                switch (tier)
                {
                    case GfxTier.Low:
                    case GfxTier.Medium:
                        exp.compensation.Override(0f);
                        exp.limitMin.Override(-5f);
                        exp.limitMax.Override(5f);
                        exp.adaptationSpeedDarkToLight.Override(1.0f);
                        exp.adaptationSpeedLightToDark.Override(1.0f);
                        break;
                    case GfxTier.High:
                    case GfxTier.Ultra:
                        exp.compensation.Override(0f);
                        exp.limitMin.Override(-5f);
                        exp.limitMax.Override(5f);
                        exp.adaptationSpeedDarkToLight.Override(1.5f);
                        exp.adaptationSpeedLightToDark.Override(1.5f);
                        break;
                }
            }
        }
    }
}