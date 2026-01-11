using LightHouse.Features.Computer.LEO.Cameras;
using UnityEngine;

namespace LightHouse.Features.Computer.LEO
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
