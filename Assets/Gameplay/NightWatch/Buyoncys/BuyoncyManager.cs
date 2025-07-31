using UnityEngine;

public class BuyoncyManager : MonoBehaviour
{
    [SerializeField] private BuyoncyController[] _buyoncies;

    private void Awake()
    {
        for (int i = 0; i < _buyoncies.Length; i++)
        {
            _buyoncies[i].BuyoncyID = i + 1;
        }
    }

    private void OnValidate()
    {
        _buyoncies = GetComponentsInChildren<BuyoncyController>();
    }
}
