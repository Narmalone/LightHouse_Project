using System;
using UnityEngine;

namespace LightHouse.Features.Sonar.Core
{
    public interface ISonarable
    {
        public string Name { get; }
        public int UniqueID { get; set; }
        public bool IsDetectedBySonar { get; set; }
        public Vector3 Position { get; }
        public Vector3 RotationAngles { get; }
        public Sprite DotSprite { get; set; }
        public Color DotColor { get; set; }
        public Vector2 DotSize { get; set; }
        public string SonarInfo { get; set; }
        public Action ForceDotUpdate{ get; set; }
    }

}
