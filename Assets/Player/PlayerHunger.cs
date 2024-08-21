using System.Collections;
using UnityEngine;

public class PlayerHunger : MonoBehaviour
{
    [SerializeField] private CustomEvent_Float _eventOnUpdateHunger;
    [SerializeField] private CustomEvent_Float _eventEat;
    [SerializeField] private float _deltaTimeToRemoveHunger;
    [SerializeField] private float _stepRemoveHunger;
    [SerializeField] private float _currentHunger;

    private WaitForSeconds _waitForHunger;
    private Coroutine _coroutineHunger;

    public float CurrentHunger
    {
        get {return _currentHunger;}
        set
        {
            _currentHunger = Mathf.Max(0,value);
            _eventOnUpdateHunger.Raise(_currentHunger);
        }
    }

    private void Awake()
    {
        _waitForHunger = new WaitForSeconds(_deltaTimeToRemoveHunger);
        _eventEat.handle += Eat;
    }

    private void Start()
    {
        _coroutineHunger = StartCoroutine(Hunger_Coroutine());
    }

    private void OnDestroy()
    {
        _eventEat.handle -= Eat;
    }

    public void Eat(float food)
    {
        CurrentHunger += food;
    }

    IEnumerator Hunger_Coroutine()
    {
        while (true)
        {
            Eat(-_stepRemoveHunger);
            yield return _waitForHunger;
        }
    }
}