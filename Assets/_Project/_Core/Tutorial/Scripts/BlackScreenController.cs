using System;
using TMPro;
using System.Collections;
using UnityEngine;

namespace LightHouse.Features.Tutorial
{
    public sealed class BlackScreenController : NotPersistentSingleton<BlackScreenController>
    {
        [Header("Black Overlay")]
        [SerializeField] private CanvasGroup _black; // noir plein écran (alpha 0..1)

        [Header("TextMeshPro")]
        [SerializeField] private TextMeshProUGUI _wakeUpText;
        [SerializeField] private CanvasGroup _wakeUpCanvasGroup;

        private Coroutine _blackscreenRoutine;
        private Coroutine _wakeUpRoutine;

        /// <summary>
        /// Démarre un fade vers targetAlpha en duration secondes.
        /// curve est optionnelle (null = linéaire). onComplete optionnel.
        /// </summary>
        public void StartFade(float targetAlpha, float duration, AnimationCurve curve = null, Action onComplete = null)
        {
            StopFade();
            _blackscreenRoutine = StartCoroutine(FadeBlackTo(targetAlpha, duration, curve, onComplete, _black));
        }

        public void FadeWakeUpText(float targetAlpha, float duration, AnimationCurve curve = null, Action onComplete = null)
        {
            StopFadeWakeUpText();
            _wakeUpRoutine = StartCoroutine(FadeBlackTo(targetAlpha, duration, curve, onComplete, _wakeUpCanvasGroup));
        }

        public void SetWakeUpText(string text)
        {
            if (_wakeUpText != null)
            {
                _wakeUpText.text = text;
            }
        }

        /// <summary>
        /// Stoppe le fade en cours (l'alpha reste où il en était).
        /// </summary>
        public void StopFade()
        {
            if (_blackscreenRoutine != null)
            {
                StopCoroutine(_blackscreenRoutine);
                _blackscreenRoutine = null;
            }
        }
        public void StopFadeWakeUpText()
        {
            if (_wakeUpRoutine != null)
            {
                StopCoroutine(_wakeUpRoutine);
                _wakeUpRoutine = null;
            }
        }

        private IEnumerator FadeBlackTo(float targetAlpha, float duration, AnimationCurve curve, Action onComplete, CanvasGroup canvasGroup)
        {
            if (canvasGroup == null)
                yield break;

            float startAlpha = canvasGroup.alpha;

            if (duration <= 0f)
            {
                SetAlpha(canvasGroup, targetAlpha);
                _blackscreenRoutine = null;
                onComplete?.Invoke();
                yield break;
            }

            float time = 0f;
            while (time < duration)
            {
                time += Time.deltaTime;
                float n = Mathf.Clamp01(time / duration);
                float f = curve != null ? curve.Evaluate(n) : n;
                SetAlpha(canvasGroup, Mathf.Lerp(startAlpha, targetAlpha, f));
                yield return null;
            }

            SetAlpha(canvasGroup, targetAlpha);
            _blackscreenRoutine = null;
            onComplete?.Invoke();
        }

        private static void SetAlpha(CanvasGroup g, float a)
        {
            if (g == null) return;
            g.alpha = a;
            g.interactable = a > 0.99f;
            g.blocksRaycasts = a > 0.99f;
        }
    }
}