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

        private Coroutine _routine;

        /// <summary>
        /// Démarre un fade vers targetAlpha en duration secondes.
        /// curve est optionnelle (null = linéaire). onComplete optionnel.
        /// </summary>
        public void StartFade(float targetAlpha, float duration, AnimationCurve curve = null, Action onComplete = null)
        {
            StopFade();
            _routine = StartCoroutine(FadeBlackTo(targetAlpha, duration, curve, onComplete));
        }

        /// <summary>
        /// Stoppe le fade en cours (l'alpha reste où il en était).
        /// </summary>
        public void StopFade()
        {
            if (_routine != null)
            {
                StopCoroutine(_routine);
                _routine = null;
            }
        }

        private IEnumerator FadeBlackTo(float targetAlpha, float duration, AnimationCurve curve, Action onComplete)
        {
            if (_black == null)
                yield break;

            float startAlpha = _black.alpha;

            if (duration <= 0f)
            {
                SetAlpha(_black, targetAlpha);
                _routine = null;
                onComplete?.Invoke();
                yield break;
            }

            float time = 0f;
            while (time < duration)
            {
                time += Time.deltaTime;
                float n = Mathf.Clamp01(time / duration);
                float f = curve != null ? curve.Evaluate(n) : n;
                SetAlpha(_black, Mathf.Lerp(startAlpha, targetAlpha, f));
                yield return null;
            }

            SetAlpha(_black, targetAlpha);
            _routine = null;
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