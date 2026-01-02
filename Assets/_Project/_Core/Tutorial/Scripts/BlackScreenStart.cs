using System.Collections;
using UnityEngine;

namespace LightHouse.Game.Tutorial
{
    public class IntroSplashController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CanvasGroup _blackCanvasGroup;

        [Header("Timing")]
        [SerializeField, Min(0f)] private float _holdBeforeFade = 2.0f;
        [SerializeField, Min(0.01f)] private float _fadeDuration = 0.8f;

        [Header("Curve (0->1)")]
        [SerializeField] private AnimationCurve _fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Behavior")]
        [SerializeField] private bool _disableAfterFade = true;
        [SerializeField] private bool _blockInputWhileVisible = true;

        private Coroutine _routine;

        private void Reset()
        {
            // Petite valeur par défaut agréable
            _fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        }

        private void OnEnable()
        {
            // Assure l'état initial
            SetAlpha(1f);
            SetBlocking(true);

            _routine = StartCoroutine(SplashRoutine());
        }

        private void OnDisable()
        {
            if (_routine != null)
                StopCoroutine(_routine);
        }

        private IEnumerator SplashRoutine()
        {
            if (_holdBeforeFade > 0f)
                yield return new WaitForSeconds(_holdBeforeFade);

            yield return FadeOut();

            SetBlocking(false);

            if (_disableAfterFade)
                gameObject.SetActive(false);
        }

        private IEnumerator FadeOut()
        {
            float t = 0f;
            while (t < _fadeDuration)
            {
                t += Time.deltaTime;
                float normalized = Mathf.Clamp01(t / _fadeDuration);

                // Curve donne 0->1, nous on veut alpha 1->0
                float curveValue = _fadeCurve.Evaluate(normalized);
                float alpha = Mathf.Lerp(1f, 0f, curveValue);

                SetAlpha(alpha);
                yield return null;
            }

            SetAlpha(0f);
        }

        private void SetAlpha(float a)
        {
            if (_blackCanvasGroup == null) return;
            _blackCanvasGroup.alpha = a;
        }

        private void SetBlocking(bool block)
        {
            if (_blackCanvasGroup == null) return;

            if (_blockInputWhileVisible)
            {
                _blackCanvasGroup.blocksRaycasts = block;
                _blackCanvasGroup.interactable = block;
            }
        }
    }
}
