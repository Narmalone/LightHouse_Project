using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeUI : MonoBehaviour
{
    public static FadeUI Instance;

    [Header("Events")]
    [SerializeField] private CustomEvent_2Float _eventStartFade;
    [SerializeField] private CustomEvent_Color _eventSetColor;
    [SerializeField] private CustomEvent _eventOnFade;
    [SerializeField] private CustomEvent _eventOnFadeIsMasking;
    [SerializeField] private CustomEvent _eventOnFadeEnd;

    [Header("Components")]
    [SerializeField] private Image _fadeImage;

    [Header("Components")]
    [SerializeField] private AnimationCurve _easeFade;
    [SerializeField] private float _fadeInDuration;

    private bool _isFadeStarted = false;

    private Coroutine _coroutineFade;
    private Color _fadeColor;

    private void Awake()
    {
        Instance = this;
        _eventStartFade.handle += OnStartFade;
        _eventSetColor.handle += OnSetColor;
    }

    private void Start()
    {
        _fadeColor = _fadeImage.color;
        Fade(1,1,0);
    }

    private void OnDestroy()
    {
        _eventStartFade.handle -= OnStartFade;
        _eventSetColor.handle -= OnSetColor;
    }

    [ContextMenu("FADE")]
    private void Fade()
    {
        OnStartFade(1, 1);
    }

    private void OnStartFade(float fadeInDuration, float duration)
    {
        if (_isFadeStarted) return;

        _eventOnFade.Raise();
        _coroutineFade = StartCoroutine(Fade_Coroutine(fadeInDuration, duration, 1, true));
    }

    private void OnSetColor(Color color)
    {
        _fadeColor.r = color.r;
        _fadeColor.g = color.g;
        _fadeColor.b = color.b;
    }

    IEnumerator Fade_Coroutine(float fadeInDuration, float duration, float fadeAmountTarget, bool fadeOut)
    {
        float time = 0;
        float fadeAmountStart = _fadeColor.a;

        while (time < fadeInDuration)
        {
            time += Time.deltaTime;
            Fade(time/ fadeInDuration, fadeAmountStart, fadeAmountTarget);
            yield return null;
        }

        if (fadeOut == false) 
        {
            _eventOnFadeEnd.Raise();
            _coroutineFade = null;
            yield break;
        }

        _eventOnFadeIsMasking.Raise();
        yield return new WaitForSeconds(duration);

        _coroutineFade = StartCoroutine(Fade_Coroutine(fadeInDuration, duration, 0, false));
    }

    private void Fade(float time, float fadeAmountStart, float fadeAmountTarget)
    {
        _fadeColor.a = Mathf.Lerp(fadeAmountStart, fadeAmountTarget, _easeFade.Evaluate(time));
        _fadeImage.color = _fadeColor;
    }
}