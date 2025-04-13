using System;
using UnityEngine;

namespace LightHouse.GrabableItems
{
    public class PlayerGrabableItems : MonoBehaviour
    {
        public static event Action<PlayerGrabableItems> OnPlayerGrabableInitialized;

        [SerializeField] private Transform _playerGrabableItemsParent;
        [SerializeField] private Transform _playerCamera;
        public Transform PlayerGrabableItemsParent => _playerGrabableItemsParent;
        public Transform PlayerCamera => _playerCamera;

        private void Start()
        {
            OnPlayerGrabableInitialized?.Invoke(this);
        }
    }
}
