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
        [SerializeField] private LocalizedAsset<SO_AudioCue> localizedCue;
        [SerializeField] private LocalizedString localizedSubtitle;

        [Header("Audio Config")]
        [Min(0f)][SerializeField] private float volume = 1f;
        [SerializeField] private bool loop = false;
        [Range(0f, 1f)][SerializeField] private float spatialBlend = 0f;

        [Header("Text Config")]
        [Tooltip("Dur�e estim�e si aucun clip n'est disponible (ou pas encore charg�).")]
        [SerializeField] private bool enableAutoFallbackDuration = false;

        [Min(0f)][SerializeField] private float fallbackDuration = 3f;
        [Min(0f)][SerializeField] private float additionalDurationOnceLetterAppeared = 1f;
        [Range(0f, 0.2f)]
        [SerializeField, Tooltip("Mettre � 0 pour afficher le texte instantan�ment.")]
        private float charDelay = 0.04f;

        [Header("Choix (optionnel)")]
        [Tooltip("Si non vide, à la fin de ce dialogue le joueur doit sélectionner un de ces choix avant que la conversation continue. Laisser vide pour un dialogue linéaire classique.")]
        [SerializeField] private TalkieChoice[] choices;

        [Header("Runtime (Debug)")]
        [SerializeField, TextArea] private string currentSubtitleText;
        [SerializeField] private SO_AudioCue currentAudioCue;

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
        public LocalizedAsset<SO_AudioCue> CueRef => localizedCue;

        public TalkieChoice[] Choices
        {
            get
            {
                // Assigne l'index de chaque choix à la volée (position dans le
                // tableau), pour que TalkieChoice.Index soit toujours fiable
                // sans dépendre d'un OnValidate éditeur uniquement.
                if (choices != null)
                {
                    for (int i = 0; i < choices.Length; i++)
                        choices[i]?.SetIndex(i);
                }

                return choices;
            }
        }

        public bool HasChoices => choices != null && choices.Length > 0;
        #endregion

        #region Localization events (optional)
        /// <summary>
        /// Appelle �a si tu veux que l�asset garde un cache debug � jour
        /// quand la locale change ou quand les assets finissent de charger.
        /// (Sinon tu peux compl�tement t�en passer.)
        /// </summary>
        public void Register()
        {
            if (isRegistered) return;
            isRegistered = true;

            if (localizedCue != null)
                localizedCue.AssetChanged += OnCueChanged;

            if (localizedSubtitle != null)
                localizedSubtitle.StringChanged += OnSubtitleChanged;

            // Optionnel : forcer une premi�re mise � jour du texte
            // (GetLocalizedString d�clenche en g�n�ral le StringChanged aussi selon config)
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

            // Si Table/Entry vide, on �vite l'exception "Empty Table Reference"
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
        /// Dur�e bas�e sur le texte (typewriter), sans n�cessiter que le cache debug soit � jour.
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
        /// Charge le AudioCue localis� (async). Le handle doit �tre release par celui qui l'a demand�.
        /// </summary>
        public AsyncOperationHandle<SO_AudioCue> LoadCueAsync()
        {
            if (localizedCue == null)
                throw new InvalidOperationException($"{name}: localizedCue is null.");

            return localizedCue.LoadAssetAsync();
        }

        /// <summary>
        /// Essaie de r�cup�rer un clip "principal" depuis le cue (ex: variante 0).
        /// </summary>
        public static AudioClip TryGetMainClip(SO_AudioCue cue)
        {
            if (cue == null) return null;
            if (cue.Variants == null || cue.Variants.Length == 0) return null;
            if (cue.Variants[0] == null) return null;
            return cue.Variants[0].Clip;
        }
        #endregion

        #region Duration
        /// <summary>
        /// Calcule une dur�e d'affichage robuste :
        /// - si le cue est d�j� connu (via AssetChanged ou cache externe) => dur�e audio
        /// - sinon => fallback bas� sur le texte (synchrone) ou dur�e fixe
        /// </summary>
        public float GetDisplayDuration(SO_AudioCue cueOverride = null)
        {
            // 1) audio si dispo
            var cue = cueOverride != null ? cueOverride : currentAudioCue;
            var clip = TryGetMainClip(cue);
            if (clip != null)
                return clip.length + additionalDurationOnceLetterAppeared;

            // 2) fallback bas� sur le texte (synchrone)
            var subtitle = GetSubtitles();
            var estimated = EstimateSubtitleDuration(subtitle);

            // 3) si estim� trop petit, fallbackDuration
            return (estimated > 0f ? estimated : fallbackDuration) + additionalDurationOnceLetterAppeared;
        }
        #endregion

        #region Event handlers + cache
        private void OnSubtitleChanged(string value) => currentSubtitleText = value;
        private void OnCueChanged(SO_AudioCue value) => currentAudioCue = value;

        private void TryRefreshSubtitleCache()
        {
            // Best-effort : rempli currentSubtitleText m�me sans events.
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
