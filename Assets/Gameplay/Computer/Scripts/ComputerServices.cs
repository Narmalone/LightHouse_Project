using LightHouse.Game.Computer.Cameras;
using UnityEngine;

namespace LightHouse.Game.Computer
{
    [System.Serializable]
    public class ComputerServices
    {
        [field: SerializeField] public LightHouseCamerasSystem CameraSystem { get; set; }
        // Tu peux ajouter d'autres services ici
        // public SonarSystem Sonar { get; set; }
        // public NightWatchController NightWatch { get; set; }
    }
}
