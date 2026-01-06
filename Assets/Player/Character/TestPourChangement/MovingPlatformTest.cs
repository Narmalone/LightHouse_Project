using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class MovingPlatformTest : MonoBehaviour
{
    [Header("Path")]
    [SerializeField] private Vector3 _localOffset = new Vector3(0f, 0f, 5f);
    [SerializeField] private float _period = 2f; // secondes aller-retour
    [SerializeField] private AnimationCurve _ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Rotation (optional)")]
    [SerializeField] private bool _rotate;
    [SerializeField] private Vector3 _angularDegreesPerSec = new Vector3(0f, 45f, 0f);

    private Rigidbody _rb;
    private Vector3 _startPos;
    private Quaternion _startRot;

    public Vector3 DeltaPosition { get; private set; }
    public Quaternion DeltaRotation { get; private set; }
    public Vector3 Velocity { get; private set; }

    private Vector3 _prevPos;
    private Quaternion _prevRot;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;

        var col = GetComponent<Collider>();
        col.isTrigger = false;

        _startPos = transform.position;
        _startRot = transform.rotation;

        _prevPos = _startPos;
        _prevRot = _startRot;
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        // t ping-pong 0..1..0
        float t01 = (_period <= 0.001f) ? 0f : Mathf.PingPong(Time.time / _period, 1f);
        float t = _ease != null ? _ease.Evaluate(t01) : t01;

        Vector3 targetPos = _startPos + transform.TransformVector(_localOffset) * t;

        Quaternion targetRot = _startRot;
        if (_rotate)
        {
            targetRot = _startRot * Quaternion.Euler(_angularDegreesPerSec * Time.time);
        }

        // Move rigidbody (physics-friendly)
        _rb.MovePosition(targetPos);
        _rb.MoveRotation(targetRot);

        // Compute deltas (after move)
        Vector3 newPos = _rb.position;
        Quaternion newRot = _rb.rotation;

        DeltaPosition = newPos - _prevPos;
        DeltaRotation = newRot * Quaternion.Inverse(_prevRot);

        Velocity = (dt > 0f) ? (DeltaPosition / dt) : Vector3.zero;

        _prevPos = newPos;
        _prevRot = newRot;
    }
}
