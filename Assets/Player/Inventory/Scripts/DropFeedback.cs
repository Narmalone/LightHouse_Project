using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DropFeedback : MonoBehaviour
{
    [SerializeField] private Image _circle;
    [SerializeField] private CustomEvent _eventStartDrop;
    [SerializeField] private CustomEvent _eventEndDrop;
    private AnimationCurve _curveStrenghtGrow;
    private float _timeToStrengthMax;
    private float _timeStart;

    private Coroutine _coroutine;

    private void Awake()
    {
        _timeToStrengthMax = PlayerManager.Instance._data._timeToAchieveMaxStrength;

        _eventStartDrop.handle += OnStartFeedback;
        _eventEndDrop.handle += OnEndFeedback;
    }
    private void OnDestroy()
    {
        _eventStartDrop.handle -= OnStartFeedback;
        _eventEndDrop.handle -= OnEndFeedback;
    }

    private void OnStartFeedback()
    {
        _timeStart = Time.time;
        _coroutine = StartCoroutine(AnimationDrop());
    }

    private void OnEndFeedback()
    {
        if (_coroutine != null) StopCoroutine(_coroutine);
    }

    IEnumerator AnimationDrop()
    {
        float time = 0;
        while (time >= 1)
        {
            time += Time.deltaTime * _timeToStrengthMax;
            yield return null;
        }
    }
}