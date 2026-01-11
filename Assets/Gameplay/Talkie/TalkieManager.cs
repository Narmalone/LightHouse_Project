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
        // --- Nouveaux events publics ---
        // Appelé juste avant de jouer un dialogue
        public event Action<LocalizedDialogueAudio> OnDialogueStarted;

        // Appelé juste après qu'un dialogue est fini d'afficher / parler
        public event Action<LocalizedDialogueAudio> OnDialogueFinished;

        // Appelé quand la file est complètement vidée
        public event Action OnQueueDrained;


        public static Action ForceTalkiePlay;
        [SerializeField] private TalkieServiceReference _talkieServiceReference;
        [SerializeField] private TextMeshProUGUI _radioTMP;
        [SerializeField] private CanvasGroup _radioDialog;

        [Header("Affichage")]
        [SerializeField] private float displayDuration = 3f;
        [SerializeField] private float fadeDuration = 0.5f;

        private readonly Queue<LocalizedDialogueAudio> _dialogueQueue = new();
        private bool _isPlaying;

        private void Awake()
        {
            _talkieServiceReference.Register(this);
        }

        private void OnDestroy()
        {
            _talkieServiceReference.ResetService();
        }


        private IEnumerator LoadLocalizedCue(LocalizedDialogueAudio dialogue, Action<AudioCue, AsyncOperationHandle<AudioCue>> onLoaded)
        {
            AsyncOperationHandle<AudioCue> handle;

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
                LocalizedDialogueAudio dialogue = _dialogueQueue.Dequeue();

                // --- signale qu'on commence ce dialogue ---
                OnDialogueStarted?.Invoke(dialogue);

                AudioCue loadedCue = null;
                AsyncOperationHandle<AudioCue> cueHandle = default;
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

                // on attend la fin d'affichage / audio
                yield return DisplayRadioText(dialogue.GetSubtitles(), dialogue.GetDisplayDuration(), dialogue.CharDelay);

                // --- signale qu'on a fini ce dialogue ---
                OnDialogueFinished?.Invoke(dialogue);
            }

            _isPlaying = false;

            // --- signale que la queue est vide ---
            OnQueueDrained?.Invoke();
        }

        private IEnumerator DisplayRadioText(string fullText, float audioDuration, float charDelay = 0.06f)
        {
            _radioTMP.text = "";
            _radioDialog.alpha = 0f;

            float remainingDisplayTime = Mathf.Max(displayDuration, audioDuration) - (fullText.Length * charDelay);

            WaitForSeconds textWFS = new WaitForSeconds(charDelay);
            WaitForSeconds remainDisplayTimeWTS = new WaitForSeconds(remainingDisplayTime);

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

            if (remainingDisplayTime > 0)
                yield return remainDisplayTimeWTS;

            // Fade-out
            t = 0f;
            while (t < fadeDuration)
            {
                _radioDialog.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
                t += Time.deltaTime;
                yield return null;
            }

            _radioDialog.alpha = 0f;
            _radioTMP.text = "";
        }

        public void Enqueue(LocalizedDialogueAudio audio)
        {
            if (audio == null)
            {
                Debug.LogWarning("[Talkie] Dialogue null.");
                return;
            }

            _dialogueQueue.Enqueue(audio);

            if (!_isPlaying)
            {
                StartCoroutine(ProcessTalkieQueue());
            }
        }

        private IAudioHandle _currentBip;
        public AudioCue _baseBipSound;
        public void Bip(AudioCue cue = null)
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
