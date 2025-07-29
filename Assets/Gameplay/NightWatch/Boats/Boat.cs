using LightHouse.Game.Computer.NightWatch.Sonar;
using UnityEngine;

public class Boat : MonoBehaviour, ISonarable
{
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private BoatsNationalitiesManager _boatsManager;
    [SerializeField] private BoatData _data;

    [SerializeField] private VectorPathDatabase _randomPointController;
    [SerializeField] private BoidController _controller;
    [SerializeField] private BoatAnomalyController _anomalyController;
    public BoatAnomalyController AnomalyController => _anomalyController;
    public Rigidbody RB => _rb;
    public BoatData Data => _data;

    [Header("Sonar Element")]
    public string Name { get; }

    public int UniqueID { get; set; }
    public bool IsDetectedBySonar { get; set; }

    public Vector3 Position => _rb.transform.position;

    public Vector3 RotationAngles => _rb.transform.eulerAngles;

    [field: SerializeField] public Color DotColor { get; set; }
    [field: SerializeField] public Vector2 DotSize { get; set; } = new Vector2(15, 15);
    [field: SerializeField] public Sprite DotSprite { get; set; }

    private void Awake()
    {
        _data = _boatsManager.Register();
        this.gameObject.name = _data.Name;
        SonarHandlerData.Register(this);
        _controller.Initialize(_randomPointController.GetRandomPath());
    }

    private void LateUpdate()
    {
        if (_controller.Progress >= 1.0f)
        {
            Destroy(this.gameObject);
        }
    }

    private void OnDestroy()
    {
        _boatsManager.Unregister(_data);
        SonarHandlerData.Unregister(this);
    }
}
