using UnityEngine;

namespace LightHouse.Game.Computer.NightWatch.Sonar
{
    public interface ISonarable
    {
        public string Name { get; }
        public bool IsDetectedBySonar { get; set; }
        public Vector3 Position { get; }
        public Color DotColor { get; set; }
        public Vector2 DotSize { get; set; }
    }

}
