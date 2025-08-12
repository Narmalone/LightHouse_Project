using System;
using UnityEngine;

namespace LightHouse.Game.Sonar.Core
{
    public class SonarItemTest : MonoBehaviour, ISonarable
    {
        [SerializeField] private string _name;

#pragma warning disable
        public event Action ForceDotUpdate;

        public string Name => _name;

        public Vector3 Position => transform.position;
        public Vector3 RotationAngles => transform.eulerAngles;
        [field: SerializeField] public bool IsDetectedBySonar { get; set; }
        [field: SerializeField] public Color DotColor { get; set; } = Color.green;
        [field: SerializeField] public Vector2 DotSize { get; set; } = new Vector2(20f, 20f);
        [field: SerializeField] public int UniqueID { get; set; } = -1;
        [field: SerializeField] public Sprite DotSprite { get; set; }
        public string SonarInfo { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private void OnEnable()
        {
            SonarHandlerData.Register(this);
        }

        private void OnDisable()
        {
            SonarHandlerData.Unregister(this);
        }
    }
}

