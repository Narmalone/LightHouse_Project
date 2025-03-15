using UnityEngine;
using LightHouse.Interactions;
using LightHouse.Inputs;
using System;

namespace LightHouse.Items.Samples
{
    public class Key : MonoBehaviour, IInteractable, IDescribable
    {
        [SerializeField] private string _keyName;

        public event Action OnDescriptionUpdated;
        public event Action OnNameUpdated;

        public string GetName()
        {
            return _keyName;
        }

        public string GetDescription()
        {
            return $"Press {InputManager.GetBindingName(InputManager.Interact)} to pick";
        }

        public void Interact()
        {
            Debug.Log("Le joueur interagit avec la clé: " + gameObject.name);
        }
    }

}
