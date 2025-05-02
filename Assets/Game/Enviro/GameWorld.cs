using UnityEngine;

public class GameWorld : MonoBehaviour
{
    [SerializeField] private Transform _playerSpawnPoint;
    private void Awake()
    {
        GameWorldHandlerData.PlayerSpawnPoint = _playerSpawnPoint;
    }
}
