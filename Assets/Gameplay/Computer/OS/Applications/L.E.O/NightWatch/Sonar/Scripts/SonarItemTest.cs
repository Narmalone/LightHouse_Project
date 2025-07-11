using UnityEngine;

namespace LightHouse.Game.Computer.NightWatch.Sonar
{
    public class SonarItemTest : MonoBehaviour, ISonarable
    {
        [SerializeField] private string _name;
        public string Name => _name;

        public Vector3 Position => transform.position;
        public Vector3 RotationAngles => transform.eulerAngles;
        [field: SerializeField] public bool IsDetectedBySonar { get; set; }
        [field: SerializeField] public Color DotColor { get; set; } = Color.green;
        [field: SerializeField] public Vector2 DotSize { get; set; } = new Vector2(20f, 20f);
        [field: SerializeField] public int UniqueID { get; set; } = -1;

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

