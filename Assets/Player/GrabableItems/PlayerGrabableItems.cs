using System;
using LightHouse.Game.BootStrap;
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

        private void Awake()
        {
            BootStrap.OnGameAssetsLoaded += GameInitiator_OnGameSceneInitialized;
        }

        private void GameInitiator_OnGameSceneInitialized()
        {
            OnPlayerGrabableInitialized?.Invoke(this);
        }

        private void OnDestroy()
        {
            BootStrap.OnGameAssetsLoaded -= GameInitiator_OnGameSceneInitialized;
        }
    }
}
