using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LightHouse.Core.Audio
{
    [CreateAssetMenu(fileName = "LocalizedDialogue_", menuName = "LightHouse/Audio/Localized Dialogue")]
    public class LocalizedDialogueAudio : ScriptableObject
    {
        [Header("Localized References")]
        [SerializeField] private LocalizedAsset<AudioCue> localizedCue;
        [SerializeField] private LocalizedString localizedSubtitle;

        [Header("Audio Config")]
        [Min(0f)][SerializeField] private float volume = 1f;
        [SerializeField] private bool loop = false;
        [Range(0f, 1f)][SerializeField] private float spatialBlend = 0f;

        [Header("Text Config")]
        [Tooltip("Durée estimée si aucun clip n'est disponible (ou pas encore chargé).")]
        [SerializeField] private bool enableAutoFallbackDuration = false;

        [Min(0f)][SerializeField] private float fallbackDuration = 3f;
        [Min(0f)][SerializeField] private float additionalDurationOnceLetterAppeared = 1f;
        [Range(0f, 0.2f)]
        [SerializeField, Tooltip("Mettre à 0 pour afficher le texte instantanément.")]
        private float charDelay = 0.04f;

        [Header("Runtime (Debug)")]
        [SerializeField, TextArea] private string currentSubtitleText;
        [SerializeField] private AudioCue currentAudioCue;

        private bool isRegistered;

        #region Public API (read-only)
        public float Volume => volume;
        public bool Loop => loop;
        public float SpatialBlend => spatialBlend;

        public bool EnableAutoFallbackDuration => enableAutoFallbackDuration;
        public float FallbackDuration => fallbackDuration;
        public float AdditionalDurationOnceLetterAppeared => additionalDurationOnceLetterAppeared;
        public float CharDelay => charDelay;

        public LocalizedString SubtitleRef => localizedSubtitle;
        public LocalizedAsset<AudioCue> CueRef => localizedCue;
        #endregion

        #region Localization events (optional)
        /// <summary>
        /// Appelle ça si tu veux que l’asset garde un cache debug à jour
        /// quand la locale change ou quand les assets finissent de charger.
        /// (Sinon tu peux complètement t’en passer.)
        /// </summary>
        public void Register()
        {
            if (isRegistered) return;
            isRegistered = true;

            if (localizedCue != null)
                localizedCue.AssetChanged += OnCueChanged;

            if (localizedSubtitle != null)
                localizedSubtitle.StringChanged += OnSubtitleChanged;

            // Optionnel : forcer une première mise à jour du texte
            // (GetLocalizedString déclenche en général le StringChanged aussi selon config)
            TryRefreshSubtitleCache();
        }

        public void Unregister()
        {
            if (!isRegistered) return;
            isRegistered = false;

            if (localizedCue != null)
                localizedCue.AssetChanged -= OnCueChanged;

            if (localizedSubtitle != null)
                localizedSubtitle.StringChanged -= OnSubtitleChanged;
        }
        #endregion

        #region Text

        public string GetSubtitles()
        {
            return localizedSubtitle?.GetLocalizedString() ?? string.Empty;
        }


        /// <summary>
        /// Async handle utilisable en coroutine : yield return handle;
        /// IMPORTANT : caller doit Release(handle) quand fini.
        /// </summary>
        public AsyncOperationHandle<string> GetSubtitlesAsync()
        {
            if (localizedSubtitle == null)
                return default;

            // Si Table/Entry vide, on évite l'exception "Empty Table Reference"
            if (localizedSubtitle.TableReference.ReferenceType == TableReference.Type.Empty ||
                localizedSubtitle.TableEntryReference.ReferenceType == TableEntryReference.Type.Empty)
                return default;

            return localizedSubtitle.GetLocalizedStringAsync();
        }

        /// <summary>
        /// Synchrone et safe (ne throw jamais). Pratique pour fallback.
        /// </summary>
        public string GetSubtitlesSafe()
        {
            if (localizedSubtitle == null)
                return string.Empty;

            if (localizedSubtitle.TableReference.ReferenceType == TableReference.Type.Empty ||
                localizedSubtitle.TableEntryReference.ReferenceType == TableEntryReference.Type.Empty)
                return string.Empty;

            try
            {
                return localizedSubtitle.GetLocalizedString();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[{name}] GetSubtitlesSafe failed: {e.Message}", this);
                return string.Empty;
            }
        }


        /// <summary>
        /// Durée basée sur le texte (typewriter), sans nécessiter que le cache debug soit à jour.
        /// </summary>
        public float EstimateSubtitleDuration(string subtitle)
        {
            if (!enableAutoFallbackDuration)
                return fallbackDuration;

            if (string.IsNullOrEmpty(subtitle) || charDelay <= 0f)
                return 0.5f; // mini lisible

            return subtitle.Length * charDelay + 0.5f;
        }
        #endregion

        #region Audio
        /// <summary>
        /// Charge le AudioCue localisé (async). Le handle doit être release par celui qui l'a demandé.
        /// </summary>
        public AsyncOperationHandle<AudioCue> LoadCueAsync()
        {
            if (localizedCue == null)
                throw new InvalidOperationException($"{name}: localizedCue is null.");

            return localizedCue.LoadAssetAsync();
        }

        /// <summary>
        /// Essaie de récupérer un clip "principal" depuis le cue (ex: variante 0).
        /// </summary>
        public static AudioClip TryGetMainClip(AudioCue cue)
        {
            if (cue == null) return null;
            if (cue.Variants == null || cue.Variants.Length == 0) return null;
            if (cue.Variants[0] == null) return null;
            return cue.Variants[0].Clip;
        }
        #endregion

        #region Duration
        /// <summary>
        /// Calcule une durée d'affichage robuste :
        /// - si le cue est déjà connu (via AssetChanged ou cache externe) => durée audio
        /// - sinon => fallback basé sur le texte (synchrone) ou durée fixe
        /// </summary>
        public float GetDisplayDuration(AudioCue cueOverride = null)
        {
            // 1) audio si dispo
            var cue = cueOverride != null ? cueOverride : currentAudioCue;
            var clip = TryGetMainClip(cue);
            if (clip != null)
                return clip.length + additionalDurationOnceLetterAppeared;

            // 2) fallback basé sur le texte (synchrone)
            var subtitle = GetSubtitles();
            var estimated = EstimateSubtitleDuration(subtitle);

            // 3) si estimé trop petit, fallbackDuration
            return (estimated > 0f ? estimated : fallbackDuration) + additionalDurationOnceLetterAppeared;
        }
        #endregion

        #region Event handlers + cache
        private void OnSubtitleChanged(string value) => currentSubtitleText = value;
        private void OnCueChanged(AudioCue value) => currentAudioCue = value;

        private void TryRefreshSubtitleCache()
        {
            // Best-effort : rempli currentSubtitleText même sans events.
            try { currentSubtitleText = GetSubtitles(); }
            catch { /* ignore */ }
        }
        #endregion

#if UNITY_EDITOR
        private void OnValidate()
        {
            volume = Mathf.Max(0f, volume);
            fallbackDuration = Mathf.Max(0f, fallbackDuration);
            additionalDurationOnceLetterAppeared = Mathf.Max(0f, additionalDurationOnceLetterAppeared);
        }
#endif
    }
}
