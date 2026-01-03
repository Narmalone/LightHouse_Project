using System.Collections;
using UnityEngine;

namespace LightHouse.Game.Tutorial
{
    public sealed class SplashSequenceController : MonoBehaviour
    {
        [Header("Black Overlay")]
        [SerializeField] private CanvasGroup _black; // noir plein écran (alpha 0..1)

        [Header("End Black Fade")]
        [SerializeField, Min(0.01f)] private float _endBlackFadeDuration = 0.5f;
        [SerializeField, Min(0f)] private float _endBlackHold = 0.1f;
        [SerializeField] private AnimationCurve _endBlackCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Studio Card")]
        [SerializeField] private CanvasGroup _studioGroup;
        [SerializeField] private RectTransform _studioScaleTarget;

        [Header("Tech Card")]
        [SerializeField] private CanvasGroup _techGroup;
        [SerializeField] private RectTransform _techScaleTarget;

        [Header("Timing")]
        //Actuellement j'ai fais x3
        [SerializeField, Min(0f)] private float _startHoldBlack = 0.25f;

        [SerializeField, Min(0f)] private float _studioFadeIn = 0.45f;
        [SerializeField, Min(0f)] private float _studioHold = 1.10f;
        [SerializeField, Min(0f)] private float _studioFadeOut = 0.45f;

        [SerializeField, Min(0f)] private float _techFadeIn = 0.45f;
        [SerializeField, Min(0f)] private float _techHold = 1.25f;
        [SerializeField, Min(0f)] private float _techFadeOut = 0.55f;

        [Header("Micro Scale")]
        [SerializeField] private float _scaleFrom = 0.985f;
        [SerializeField] private float _scaleTo = 1.0f;

        [Header("Curves (0->1)")]
        [SerializeField] private AnimationCurve _fadeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private AnimationCurve _fadeOutCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private AnimationCurve _scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Skip")]
        [SerializeField] private bool _allowSkip = true;
        [SerializeField] private KeyCode _skipKey = KeyCode.Space;

        [Header("End Behavior")]
        [SerializeField] private bool _disableAfter = true;

        private Coroutine _routine;
        private bool _skipRequested;

        private void OnEnable()
        {
            InitState();
            _routine = StartCoroutine(RunSequence());
        }

        private void OnDisable()
        {
            if (_routine != null) StopCoroutine(_routine);
        }

        private void Update()
        {
            if (!_allowSkip) return;

            // Simple & efficace : clavier + clic + submit (si tu veux, on branchera le new Input System après)
            if (Input.GetKeyDown(_skipKey) || Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return))
                _skipRequested = true;
        }

        private void InitState()
        {
            _skipRequested = false;

            SetAlpha(_black, 1f);

            SetAlpha(_studioGroup, 0f);
            SetScale(_studioScaleTarget, _scaleFrom);

            SetAlpha(_techGroup, 0f);
            SetScale(_techScaleTarget, _scaleFrom);
        }

        private IEnumerator RunSequence()
        {
            if (_startHoldBlack > 0f && !IsSkipping())
                yield return new WaitForSeconds(_startHoldBlack);

            // --- Studio "presents" ---
            yield return ShowCard(_studioGroup, _studioScaleTarget, _studioFadeIn, _studioHold, _studioFadeOut);
            if (IsSkipping()) yield return FastExit();

            // Petite respiration (optionnel, ultra court)
            yield return null;

            // --- Tech "Made with" ---
            yield return ShowCard(_techGroup, _techScaleTarget, _techFadeIn, _techHold, _techFadeOut);
            if (IsSkipping()) yield return FastExit();

            yield return FadeBlackTo(0.0f, _endBlackFadeDuration, _endBlackCurve);

            if (_endBlackHold > 0f && !IsSkipping())
                yield return new WaitForSeconds(_endBlackHold);

            if (_disableAfter)
                gameObject.SetActive(false);
        }

        private IEnumerator FadeBlackTo(float targetAlpha, float duration, AnimationCurve curve)
        {
            if (_black == null)
                yield break;

            float startAlpha = _black.alpha;

            if (duration <= 0f)
            {
                SetAlpha(_black, targetAlpha);
                yield break;
            }

            float time = 0f;
            while (time < duration)
            {
                time += Time.deltaTime;
                float n = Mathf.Clamp01(time / duration);
                float f = curve.Evaluate(n);

                SetAlpha(_black, Mathf.Lerp(startAlpha, targetAlpha, f));
                yield return null;
            }

            SetAlpha(_black, targetAlpha);
        }


        private IEnumerator ShowCard(CanvasGroup group, RectTransform scaleTarget, float fadeIn, float hold, float fadeOut)
        {
            // Fade in + scale up
            yield return FadeAndScale(group, scaleTarget, 0f, 1f, _scaleFrom, _scaleTo, fadeIn, _fadeInCurve, _scaleCurve);
            if (IsSkipping()) yield break;

            if (hold > 0f)
                yield return WaitSkippable(hold);
            if (IsSkipping()) yield break;

            // Fade out (scale reste à 1.0, ça fait plus premium)
            yield return FadeOnly(group, 1f, 0f, fadeOut, _fadeOutCurve);
        }

        private IEnumerator FadeAndScale(CanvasGroup g, RectTransform t, float a0, float a1, float s0, float s1,
                                         float duration, AnimationCurve fadeCurve, AnimationCurve scaleCurve)
        {
            if (duration <= 0f)
            {
                SetAlpha(g, a1);
                SetScale(t, s1);
                yield break;
            }

            float time = 0f;
            while (time < duration)
            {
                if (IsSkipping()) yield break;

                time += Time.deltaTime;
                float n = Mathf.Clamp01(time / duration);

                float fa = fadeCurve.Evaluate(n);
                float fs = scaleCurve.Evaluate(n);

                SetAlpha(g, Mathf.Lerp(a0, a1, fa));
                SetScale(t, Mathf.Lerp(s0, s1, fs));

                yield return null;
            }

            SetAlpha(g, a1);
            SetScale(t, s1);
        }

        private IEnumerator FadeOnly(CanvasGroup g, float a0, float a1, float duration, AnimationCurve curve)
        {
            if (duration <= 0f)
            {
                SetAlpha(g, a1);
                yield break;
            }

            float time = 0f;
            while (time < duration)
            {
                if (IsSkipping()) yield break;

                time += Time.deltaTime;
                float n = Mathf.Clamp01(time / duration);
                float f = curve.Evaluate(n);

                SetAlpha(g, Mathf.Lerp(a0, a1, f));
                yield return null;
            }

            SetAlpha(g, a1);
        }

        private IEnumerator WaitSkippable(float seconds)
        {
            float t = 0f;
            while (t < seconds)
            {
                if (IsSkipping()) yield break;
                t += Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator FastExit()
        {
            // Coupe les cards rapidement
            SetAlpha(_studioGroup, 0f);
            SetAlpha(_techGroup, 0f);

            // Fade vers noir (au lieu de snap)
            yield return FadeBlackTo(1f, Mathf.Min(0.2f, _endBlackFadeDuration), _endBlackCurve);

            if (_disableAfter)
                gameObject.SetActive(false);
        }


        private bool IsSkipping() => _allowSkip && _skipRequested;

        private static void SetAlpha(CanvasGroup g, float a)
        {
            if (g == null) return;
            g.alpha = a;
            g.interactable = a > 0.99f;
            g.blocksRaycasts = a > 0.99f;
        }

        private static void SetScale(RectTransform t, float s)
        {
            if (t == null) return;
            t.localScale = Vector3.one * s;
        }
    }
}
