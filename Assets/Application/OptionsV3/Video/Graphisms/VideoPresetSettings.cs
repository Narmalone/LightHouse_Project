using UnityEngine;

public enum EAntiAliasing { Disabled = 0, X2 = 2, X4 = 4, X8 = 8 }

[CreateAssetMenu(fileName = "Video_Setting_", menuName = "LightHouse/Options/New Preset")]
public class VideoPresetSettings : ScriptableObject
{
    public GfxTier Tier;
    public int QualityIndex;

    [Header("Textures")]
    [Tooltip("0 = pleine résolution, 1 = 1/2, 2 = 1/4, 3 = 1/8")]
    [Range(0, 3)] public int MasterTextureLimit = 0;

    [Tooltip("AnisotropicFiltering global (Disable / Enable / ForceEnable)")]
    public AnisotropicFiltering FilteringMode = AnisotropicFiltering.Enable;

    [Tooltip("AA MSAA (0/2/4/8). Sur HDRP, ignoré si TAA activé côté pipeline.")]
    public EAntiAliasing AliasingLevel = EAntiAliasing.X4;

    [Header("Shadows")]
    [Min(0f)]
    [Tooltip("Distance de rendu des ombres en unités monde.")]
    public float ShadowDistance = 100f;

    [Tooltip("Résolution des ombres (Low/Medium/High/VeryHigh).")]
    public ShadowResolution ShadowResolution = ShadowResolution.High;

    [Tooltip("0, 2 ou 4 cascades (autres valeurs seront clampées).")]
    [Range(0, 4)] public int ShadowCascades = 2;

    [Tooltip("Shadowmask vs DistanceShadowmask (Mixed Lighting).")]
    public ShadowmaskMode ShadowMaskMode = ShadowmaskMode.DistanceShadowmask;

    [Header("LOD")]
    [Min(0.25f)]
    [Tooltip("1 = normal ; >1 = plus détaillé ; <1 = plus agressif.")]
    public float LodBias = 1.0f;

    [Min(0)]
    [Tooltip("LOD max autorisé (0 = le plus détaillé).")]
    public int MaximumLodLevel = 0;

    [Header("VSync")]
    [Tooltip("0 = Off, 1 = 1 v-blank, 2 = 2 v-blanks.")]
    [Range(0, 2)] public int VSyncCount = 1;

    [Header("Particles")]
    [Tooltip("Budget de raycasts particules (perf).")]
    [Min(0)] public int ParticleRaycastBudget = 256;

    [Header("Skinning")]
    [Tooltip("Influences par vertex (TwoBones/FourBones/Unlimited).")]
    public SkinWeights SkinWeights = SkinWeights.FourBones;

    // --- Application directe aux QualitySettings ---
    [ContextMenu("Apply To QualitySettings (Editor)")]
    public void ApplyToQualitySettings()
    {
        // Textures / Filtering / MSAA
        QualitySettings.globalTextureMipmapLimit = MasterTextureLimit;
        QualitySettings.anisotropicFiltering = FilteringMode;
        QualitySettings.antiAliasing = (int)AliasingLevel;

        // Shadows
        QualitySettings.shadowDistance = ShadowDistance;
        QualitySettings.shadowResolution = ShadowResolution;

        // Clamp propre pour cascades : Unity ne supporte que 0/2/4
        int casc = ShadowCascades <= 0 ? 0 : (ShadowCascades <= 2 ? 2 : 4);
        QualitySettings.shadowCascades = casc;

        QualitySettings.shadowmaskMode = ShadowMaskMode;

        // LOD
        QualitySettings.lodBias = LodBias;
        QualitySettings.maximumLODLevel = Mathf.Max(0, MaximumLodLevel);

        // VSync
        QualitySettings.vSyncCount = Mathf.Clamp(VSyncCount, 0, 2);

        // Particles
        QualitySettings.particleRaycastBudget = Mathf.Max(0, ParticleRaycastBudget);

        // Skinning
        QualitySettings.skinWeights = SkinWeights;
    }
}
