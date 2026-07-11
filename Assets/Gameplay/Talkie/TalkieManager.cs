using LightHouse.Core.Audio;
using LightHouse.Core.Player;
using LightHouse.Core.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace LightHouse.Features.Talkie
{
    public class TalkieManager : MonoBehaviour, ITalkieService
    {
        // Appelé juste avant de jouer un dialogue
        public event Action<LocalizedDialogueAudio> OnDialogueStarted;

        // Appelé juste après qu'un dialogue est fini d'afficher / parler
        public event Action<LocalizedDialogueAudio> OnDialogueFinished;

        // Appelé quand la file est complètement vidée
        public event Action OnQueueDrained;

        // Appelé quand un dialogue avec des choix attend une sélection du joueur
        public event Action<TalkieChoice[]> OnChoicesPresented;

        // Appelé une fois qu'un choix a été sélectionné par le joueur
        public event Action<TalkieChoice> OnChoiceSelected;


        public static Action ForceTalkiePlay;
        [SerializeField] private TalkieServiceReference _talkieServiceReference;
        [SerializeField] private TextMeshProUGUI _radioTMP;
        [SerializeField] private CanvasGroup _radioDialog;

        [Header("Choix")]
        [Tooltip("Composant qui affiche les choix et récupère la sélection du joueur. Optionnel : si un dialogue a des choix mais qu'aucun presenter n'est assigné, un warning est loggé et la conversation s'arrête là.")]
        [SerializeField] private TalkieChoicePresenter _choicePresenter;

        [Header("Affichage")]
        [SerializeField] private float displayDuration = 3f;
        [SerializeField] private float fadeDuration = 0.5f;

        /// <summary>
        /// Un élément de la file : le dialogue à jouer, plus un callback
        /// optionnel invoqué avec le TalkieChoice sélectionné si ce dialogue
        /// (ou un de ses enfants directs) a des choix. Permet à l'appelant de
        /// Enqueue(...) de récupérer proprement le résultat sans passer par un
        /// event global partagé par toute la conversation.
        /// </summary>
        private readonly struct TalkieQueueEntry
        {
            public readonly LocalizedDialogueAudio Dialogue;
            public readonly Action<TalkieChoice> OnChoiceSelected;

            public TalkieQueueEntry(LocalizedDialogueAudio dialogue, Action<TalkieChoice> onChoiceSelected)
            {
                Dialogue = dialogue;
                OnChoiceSelected = onChoiceSelected;
            }
        }

        private readonly Queue<TalkieQueueEntry> _dialogueQueue = new();
        private bool _isPlaying;

        private void Awake()
        {
            _talkieServiceReference.Register(this);
        }

        private void OnDestroy()
        {
            _talkieServiceReference.ResetService();
        }


        private IEnumerator LoadLocalizedCue(LocalizedDialogueAudio dialogue, Action<SO_AudioCue, AsyncOperationHandle<SO_AudioCue>> onLoaded)
        {
            AsyncOperationHandle<SO_AudioCue> handle;

            try
            {
                handle = dialogue.LoadCueAsync();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Talkie] LoadCueAsync failed for {dialogue.name}: {e.Message}");
                onLoaded?.Invoke(null, default);
                yield break;
            }

            yield return handle;

            if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
            {
                Debug.LogWarning($"[Talkie] Failed to load AudioCue for {dialogue.name}");
                Addressables.Release(handle);
                onLoaded?.Invoke(null, default);
                yield break;
            }

            onLoaded?.Invoke(handle.Result, handle);
        }


        private IEnumerator ProcessTalkieQueue()
        {
            _isPlaying = true;

            while (_dialogueQueue.Count > 0)
            {
                TalkieQueueEntry entry = _dialogueQueue.Dequeue();

                // PlayNode gère aussi bien un dialogue simple qu'un dialogue avec
                // des choix (auquel cas il attend la sélection puis enchaîne
                // récursivement sur la/les suite(s) choisie(s)). Ça permet à un
                // seul item de la queue de dérouler tout un arbre de conversation.
                yield return PlayNode(entry.Dialogue, entry.OnChoiceSelected);
            }

            _isPlaying = false;

            // --- signale que la queue est vide ---
            OnQueueDrained?.Invoke();
        }

        /// <summary>
        /// Joue un dialogue (audio + sous-titres), puis, s'il a des choix,
        /// attend la sélection du joueur et enchaîne sur le(s) dialogue(s)
        /// suivant(s) — récursivement, sans limite de profondeur.
        /// </summary>
        /// <param name="onChoiceSelected">
        /// Callback fourni à Enqueue(...) pour CE dialogue précis. N'est PAS
        /// propagé aux dialogues enfants joués après un choix : chaque nœud
        /// plus profond gère son propre résultat via TalkieChoice.OnChosen
        /// (data-driven) ou via l'event global OnChoiceSelected du manager.
        /// </param>
        private IEnumerator PlayNode(LocalizedDialogueAudio dialogue, Action<TalkieChoice> onChoiceSelected = null)
        {
            // --- signale qu'on commence ce dialogue ---
            OnDialogueStarted?.Invoke(dialogue);

            SO_AudioCue loadedCue = null;
            AsyncOperationHandle<SO_AudioCue> cueHandle = default;
            bool hasHandle = false;

            if (ServiceLocator.Audio != null)
            {
                yield return LoadLocalizedCue(dialogue, (cue, handle) =>
                {
                    loadedCue = cue;
                    cueHandle = handle;
                    hasHandle = handle.IsValid();
                });

                if (loadedCue != null)
                {
                    // Si tu as des options de play, mets-les ici
                    ServiceLocator.Audio.PlayOneShot(loadedCue);
                }
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(_radioTMP.rectTransform.parent as RectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_radioTMP.rectTransform);

            // on attend la fin d'affichage / audio.
            // Si ce dialogue a des choix, le texte ne doit PAS disparaître tout
            // seul : il doit rester affiché pendant que le joueur choisit.
            // C'est PresentChoicesAndContinue qui décidera quand le masquer.
            yield return DisplayRadioText(dialogue.GetSubtitles(), dialogue.GetDisplayDuration(), dialogue.CharDelay, autoHide: !dialogue.HasChoices);

            // --- signale qu'on a fini ce dialogue ---
            OnDialogueFinished?.Invoke(dialogue);

            if (dialogue.HasChoices)
            {
                yield return PresentChoicesAndContinue(dialogue.Choices, onChoiceSelected);
            }
        }

        /// <summary>
        /// Affiche les choix via _choicePresenter, attend une sélection, déclenche
        /// le UnityEvent du choix, puis rejoue récursivement les dialogues suivants.
        /// </summary>
        private IEnumerator PresentChoicesAndContinue(TalkieChoice[] choices, Action<TalkieChoice> onChoiceSelected)
        {
            if (_choicePresenter == null)
            {
                Debug.LogWarning("[Talkie] Ce dialogue a des choix mais aucun ITalkieChoicePresenter n'est assigné sur TalkieManager.");
                yield break;
            }

            TalkieChoice selected = null;

            OnChoicesPresented?.Invoke(choices);
            _choicePresenter.Present(choices, choice => selected = choice);

            yield return new WaitUntil(() => selected != null);

            _choicePresenter.Hide();
            OnChoiceSelected?.Invoke(selected);        // event global (observabilité, debug, UI...)
            selected.OnChosen?.Invoke();               // callback data-driven configuré sur le choix (Inspector)
            onChoiceSelected?.Invoke(selected);         // callback fourni à Enqueue(...) pour ce dialogue précis

            // Le texte était resté affiché pendant que le joueur choisissait
            // (voir PlayNode / DisplayRadioText autoHide=false) : maintenant
            // que le choix est tranché, on le referme proprement.
            yield return FadeOutRadioText();

            if (selected.NextDialogues != null)
            {
                foreach (LocalizedDialogueAudio next in selected.NextDialogues)
                {
                    if (next != null)
                        yield return PlayNode(next);
                }
            }
        }

        /// <param name="autoHide">
        /// Si false, le texte reste affiché indéfiniment une fois le typewriter
        /// terminé (pas d'attente, pas de fade-out) : utilisé pour les dialogues
        /// avec choix, où c'est PresentChoicesAndContinue qui gère la fermeture
        /// via FadeOutRadioText une fois le joueur sélectionné.
        /// </param>
        private IEnumerator DisplayRadioText(string fullText, float audioDuration, float charDelay = 0.06f, bool autoHide = true)
        {
            _radioTMP.text = "";
            _radioDialog.alpha = 0f;

            float remainingDisplayTime = Mathf.Max(displayDuration, audioDuration) - (fullText.Length * charDelay);

            WaitForSeconds textWFS = new WaitForSeconds(charDelay);

            // Fade-in
            float t = 0f;
            while (t < fadeDuration)
            {
                _radioDialog.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
                t += Time.deltaTime;
                yield return null;
            }
            _radioDialog.alpha = 1f;

            // Typewriter ou full text direct
            if (charDelay <= 0f)
            {
                _radioTMP.text = fullText;
            }
            else
            {
                for (int i = 0; i <= fullText.Length; i++)
                {
                    _radioTMP.text = fullText.Substring(0, i);
                    yield return textWFS;
                }
            }

            if (!autoHide)
                yield break;

            if (remainingDisplayTime > 0)
                yield return new WaitForSeconds(remainingDisplayTime);

            yield return FadeOutRadioText();
        }

        /// <summary>
        /// Fait disparaître (fade + clear) le texte radio actuellement affiché.
        /// Partagé entre le flux normal (fin de dialogue) et le flux choix
        /// (fin de sélection).
        /// </summary>
        private IEnumerator FadeOutRadioText()
        {
            float startAlpha = _radioDialog.alpha;
            float t = 0f;
            while (t < fadeDuration)
            {
                _radioDialog.alpha = Mathf.Lerp(startAlpha, 0f, t / fadeDuration);
                t += Time.deltaTime;
                yield return null;
            }

            _radioDialog.alpha = 0f;
            _radioTMP.text = "";
        }

        public void Enqueue(LocalizedDialogueAudio audio) => Enqueue(audio, null);

        /// <summary>
        /// Enqueue un dialogue. Si ce dialogue a des choix, onChoiceSelected
        /// est appelé exactement une fois, avec le TalkieChoice sélectionné
        /// (choice.Index = position 0-based dans le tableau Choices).
        /// Si le dialogue n'a pas de choix, onChoiceSelected n'est jamais appelé.
        /// </summary>
        public void Enqueue(LocalizedDialogueAudio audio, Action<TalkieChoice> onChoiceSelected)
        {
            if (audio == null)
            {
                Debug.LogWarning("[Talkie] Dialogue null.");
                return;
            }

            _dialogueQueue.Enqueue(new TalkieQueueEntry(audio, onChoiceSelected));

            if (!_isPlaying)
            {
                StartCoroutine(ProcessTalkieQueue());
            }
        }

        private IAudioHandle _currentBip;
        public SO_AudioCue _baseBipSound;
        public void Bip(SO_AudioCue cue = null)
        {
            AudioPlayOptions options = new AudioPlayOptions();
            options.FollowTransform = true;
            options.Owner = PlayerHandlerData.MainPlayer.Character.gameObject;
            _currentBip = ServiceLocator.Audio.PlayAt(cue == null ? _baseBipSound : cue, transform.position, options);
        }

        public void StopBip(float fadeOut = 1.0f)
        {
            if (_currentBip == null) return;
            _currentBip.Stop(fadeOut);
        }
    }
}
