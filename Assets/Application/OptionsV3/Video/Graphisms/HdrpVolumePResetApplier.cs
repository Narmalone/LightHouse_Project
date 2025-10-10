using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public enum GfxTier { Low, Medium, High, Ultra }

public class HdrpVolumePresetApplier : MonoBehaviour
{
    [Header("Ton Global Volume")]
    public Volume globalVolume;

    public int currentTier;

    private void Start()
    {
        currentTier = -1;
    }

    bool Get<T>(out T c) where T : VolumeComponent
    {
        c = null;
        return globalVolume && globalVolume.profile && globalVolume.profile.TryGet(out c);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.CapsLock))
        {
            currentTier++;
            if (currentTier >= Enum.GetValues(typeof(GfxTier)).Length) currentTier = 0;
            Apply((GfxTier)currentTier);
        }
    }

    public void Apply(GfxTier tier)
    {
        ApplyFog(tier);
        ApplyVolumetricClouds(tier);
        ApplySky(tier);
        ApplySSGI_GlobalIllumination(tier);
        ApplySSR(tier);
        ApplyHDShadowSettings(tier);
        ApplyMicroShadowing(tier);
        ApplyContactShadows(tier);
        ApplyExposure(tier);

        Debug.Log($"[HDRP Preset] {tier} appliqué sur {globalVolume?.profile?.name}");
    }

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
        {
            sky.active = true;
            // Coût faible : on garde actif sur tous les tiers
            // (si tu animes le ciel, réduis la fréquence d’update ailleurs pour Low)
        }
        if (Get<VisualEnvironment>(out var ve))
        {
            ve.active = true;
            // Facultatif : s’assurer qu’on utilise bien le PBSky
            // ve.skyType.Override((int)SkyType.PhysicallyBased);
        }
    }

    void ApplySSGI_GlobalIllumination(GfxTier tier)
    {
        if (Get<GlobalIllumination>(out var gi)) // <- le nom affiché dans ton screen
        {
            switch (tier)
            {
                case GfxTier.Low:
                    gi.active = false; // SSGI Off
                    break;
                case GfxTier.Medium:
                    gi.active = true;
                    gi.quality.Override(0);       // Low
                    gi.fullResolution = false;
                    gi.denoise = true;
                    break;
                case GfxTier.High:
                    gi.active = true;
                    gi.quality.Override(1);       // Medium
                    gi.fullResolution = false;
                    gi.denoise = true;
                    break;
                case GfxTier.Ultra:
                    gi.active = true;
                    gi.quality.Override(2);       // High
                    gi.fullResolution = true;
                    gi.denoise = true;
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
                    ssr.quality.Override(0);            // Low
                    ssr.fullResolution  = false;
                    ssr.depthBufferThickness.Override(0.2f);
                    break;
                case GfxTier.High:
                    ssr.active = true;
                    ssr.quality.Override(1);            // Medium
                    ssr.fullResolution = false;
                    ssr.depthBufferThickness.Override(0.25f);
                    break;
                case GfxTier.Ultra:
                    ssr.active = true;
                    ssr.quality.Override(2);            // High
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

            // Ce composant pilote des limites côté volume (le gros du coût reste dans le RP Asset).
            switch (tier)
            {
                case GfxTier.Low:
                    sh.maxShadowDistance.Override(40f);
                    sh.cascadeShadowSplitCount.Override(1); // 1 cascade (ou 0 selon version)
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
