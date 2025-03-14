using UnityEngine;
using LightHouse.Interactions;
using LightHouse.Inputs;

namespace LightHouse.Items
{
    public class Key : MonoBehaviour, IInteractable, IDescribable
    {
        [SerializeField] private string _keyName;

        public string GetName()
        {
            return _keyName;
        }

        public string GetDescription()
        {
            return $"Press {InputUtility.GetBindingName(InputUtility.Interact)} to pick";
        }

        public void Interact()
        {
            Debug.Log("Le joueur interagit avec la clé: " + gameObject.name);
        }
    }

}
