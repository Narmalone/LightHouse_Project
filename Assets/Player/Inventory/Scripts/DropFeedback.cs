using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DropFeedback : MonoBehaviour
{
    [SerializeField] private Image _circle;
    [SerializeField] private CustomEvent_2Float _eventStartDrop;
    [SerializeField] private CustomEvent _eventEndDrop;
    private float _timeToStrengthMax;
    private Coroutine _coroutine;

    private void Awake()
    {
        _eventStartDrop.handle += OnStartFeedback;
        _eventEndDrop.handle += OnEndFeedback;
    }

    private void OnDestroy()
    {
        _eventStartDrop.handle -= OnStartFeedback;
        _eventEndDrop.handle -= OnEndFeedback;
    }

    private void OnStartFeedback(float value, float startValue)
    {
        _timeToStrengthMax = value;
        _circle.gameObject.SetActive(true);
        _coroutine = StartCoroutine(AnimationDrop(startValue));
    }

    private void OnEndFeedback()
    {
        if (_coroutine != null) StopCoroutine(_coroutine);
        _circle.gameObject.SetActive(false);
    }

    IEnumerator AnimationDrop(float startValue)
    {
        float time = startValue;
        _circle.fillAmount = time;
        while (time < 1)
        {
            time += Time.deltaTime / _timeToStrengthMax;
            _circle.fillAmount = time;
            yield return null;
        }
    }
}