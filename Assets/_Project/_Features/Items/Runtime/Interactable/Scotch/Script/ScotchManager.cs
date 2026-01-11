using UnityEngine;

namespace LightHouse.Features.Items.Interactable.Scotch
{
    public class ScotchManager : MonoBehaviour
    {
        [SerializeField] private ScotchNeeder[] _scotchNeeder;
        private int _scotchNeederCount;

        private void Awake()
        {
            _scotchNeederCount = _scotchNeeder.Length;
            foreach (var scotchNeeder in _scotchNeeder)
            {
                scotchNeeder.OnItemUsedOnMe += ScotchNeeder_OnItemUsed;
            }
        }

        private void OnDestroy()
        {
            foreach (var scotchNeeder in _scotchNeeder)
            {
                scotchNeeder.OnItemUsedOnMe -= ScotchNeeder_OnItemUsed;
            }
        }

        private void ScotchNeeder_OnItemUsed()
        {
            _scotchNeederCount--;
            if (_scotchNeederCount <= 0)
            {
                print("Tout est bouché !");
            }
        }
    }
}
