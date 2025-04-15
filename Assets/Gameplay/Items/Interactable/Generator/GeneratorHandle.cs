using LightHouse.Interactions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LightHouse.Items
{
    public class GeneratorHandle : MonoBehaviour, IInteractable
    {
        public bool CanBeInteracted { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool IsItemRaycasted { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool CanBeRaycasted { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public event Action OnObjectInteracted;
        public event Action OnInteractionNameChanged;
        public event Action OnNameUpdated;

        public Collider GetCollider()
        {
            throw new NotImplementedException();
        }

        public GameObject GetGameObject()
        {
            throw new NotImplementedException();
        }

        public string GetInteractionName()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            throw new NotImplementedException();
        }

        public void Interact()
        {
            throw new NotImplementedException();
        }
    }
}
