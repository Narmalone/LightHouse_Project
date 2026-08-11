using LightHouse.Features.Items.Interactable;
using System;
using System.Collections;
using UnityEngine;

public class FuzeButtonController : InteractableItemBase
{
    public event Action OnFuzeButtonPressed;
    [SerializeField] private Transform _fuzePartTransform;
    [SerializeField] private Transform _fuzeTargetPoint;
    private Vector3 _initialPosition;
    private bool _isFuzeOut = false;

    private Coroutine _fuzeRoutine;

    protected override void Awake()
    {
        base.Awake();
        _initialPosition = _fuzePartTransform.position;
    }

    private void OnFuzePressed()
    {
        _isFuzeOut = !_isFuzeOut;
        if(_fuzeRoutine != null)
        {
            StopCoroutine(_fuzeRoutine);
        }   
        _fuzeRoutine = StartCoroutine(GoTo(_isFuzeOut));
        OnFuzeButtonPressed?.Invoke();

    }
    private IEnumerator GoTo(bool toTarget)
    {
        Vector3 targetPosition = toTarget ? _fuzeTargetPoint.position : _initialPosition;
        while (Vector3.Distance(_fuzePartTransform.position, targetPosition) > 0.01f)
        {
            _fuzePartTransform.position = Vector3.Lerp(_fuzePartTransform.position, targetPosition, Time.deltaTime);
            yield return null;
        }

        _fuzeRoutine = null;
    }

    public override void Interact()
    {
        OnFuzePressed();
    }
}
