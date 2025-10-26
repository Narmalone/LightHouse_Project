using LightHouse.Handlers;
using System.Collections;
using UnityEngine;

public class TutorialBoat : MonoBehaviour
{
    [SerializeField] private BoatPathMover _boid;
    [SerializeField] private VectorPath _targetPath;
    [SerializeField] private Transform _playerSpawnPoint;

    private void Start()
    {
        if (PlayerHandlerData.MainPlayer != null && PlayerHandlerData.MainPlayer.Character != null)
        {
            PlayerHandlerData.MainPlayer.Character.SetPosition(_playerSpawnPoint.position);
        }
    }
}
