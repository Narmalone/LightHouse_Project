using UnityEngine;
using LightHouse.Interactions;
using System;
using LightHouse.GrabableItems;
using LightHouse.Inputs;

[RequireComponent(typeof(Rigidbody), typeof(SpringJoint))]
public class GrabableItemSpring : MonoBehaviour, IInteractable
{
    [Header("Spring Joint Settings")]
    [SerializeField] private float baseSpring = 600f;
    [SerializeField] private float baseDamper = 300f;
    [SerializeField] private float maxSpring = 2000f;
    [SerializeField] private float maxDamper = 2000f;

    [SerializeField] private float _maxDistanceBetweenPlayerAndItem = 1.3f;
    [SerializeField] private float maxDistance = 0.3f;
    [SerializeField] private float minDistance = 0f;
    [SerializeField] private float tolerance = 0.01f;

    [Header("Dynamic Adjustment")]
    [SerializeField] private float criticalDistance = 2.5f; // si l'objet est trop loin → spring très fort
    [SerializeField] private float closeDistance = 0.5f;    // si l'objet est très proche → damper très fort

    private Transform _grabableAnchor;
    private Transform _playerCam;

    [SerializeField] private SpringJoint springJoint;
    [SerializeField] private Rigidbody rb;

    public bool CanBeInteracted { get; set; } = true;
    public bool IsItemRaycasted { get; set; }
    [field: SerializeField] public bool CanBeRaycasted { get; set; } = true;

    public event Action OnObjectInteracted;
    public event Action OnInteractionNameChanged;
    public event Action OnNameUpdated;

    [SerializeField] private bool _isDraging = false;

    private void Awake()
    {
        PlayerGrabableItems.OnPlayerGrabableInitialized += PlayerGrabableItems_OnPlayerGrabableInitialized;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    private void PlayerGrabableItems_OnPlayerGrabableInitialized(PlayerGrabableItems obj)
    {
        _grabableAnchor = obj.PlayerGrabableItemsParent;
        _playerCam = obj.PlayerCamera;
    }

    public void Interact()
    {
        _isDraging = !_isDraging;

        if (_isDraging)
        {
            springJoint.autoConfigureConnectedAnchor = false;
            springJoint.connectedAnchor = _grabableAnchor.position;

            springJoint.spring = baseSpring;
            springJoint.damper = baseDamper;
            springJoint.maxDistance = maxDistance;
            springJoint.minDistance = minDistance;
            springJoint.tolerance = tolerance;

            springJoint.enableCollision = false;
            springJoint.breakForce = Mathf.Infinity;
        }
        else
        {
            springJoint.spring = 0f;
            springJoint.damper = 0f;
        }

        OnInteractionNameChanged?.Invoke();
    }

    private void FixedUpdate()
    {
        if (!_isDraging) return;

        if (springJoint != null && _grabableAnchor != null)
        {
            springJoint.connectedAnchor = _grabableAnchor.position;

            float distance = Vector3.Distance(transform.position, _grabableAnchor.position);
            // Calcul dynamique du spring
            if (distance > criticalDistance)
            {
                springJoint.spring = maxSpring;
                springJoint.damper = baseDamper; // laisser le damper normal
            }
            else if (distance <= closeDistance)
            {
                springJoint.spring = baseSpring;
                springJoint.damper = maxDamper; // amorti fort si très proche
            }
            else
            {
                // Interpolation linéaire entre les deux
                float t = Mathf.InverseLerp(closeDistance, criticalDistance, distance);
                springJoint.spring = Mathf.Lerp(baseSpring, maxSpring, t);
                springJoint.damper = Mathf.Lerp(maxDamper, baseDamper, t);
            }

            Vector3 camForward = _playerCam.forward;
            Vector3 flatCamForward = Vector3.ProjectOnPlane(camForward, Vector3.up).normalized;

            // 2. Vérifie qu'on a bien une direction valide
            if (flatCamForward.sqrMagnitude > 0.001f)
            {
                // 3. Regarder dans la direction opposée à la caméra sur le plan horizontal
                Quaternion targetRotation = Quaternion.LookRotation(flatCamForward);

                // 4. Rotation lissée
                Quaternion smoothedRotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 10f);

                // 5. Appliquer via Rigidbody
                rb.MoveRotation(smoothedRotation);
            }

            if (distance >= _maxDistanceBetweenPlayerAndItem)
            {
                _isDraging = false;
                springJoint.spring = baseSpring;
                springJoint.damper = maxDamper; // amorti fort si très proche
            }
        }
    }

    private void OnDestroy()
    {
        PlayerGrabableItems.OnPlayerGrabableInitialized -= PlayerGrabableItems_OnPlayerGrabableInitialized;
    }

    public string GetInteractionName() => _isDraging ? $"{InputManager.GetBindingName(InputManager.Interact)} to stop" : $"{InputManager.GetBindingName(InputManager.Interact)} to drag";
    public string GetName() => "";
    public GameObject GetGameObject() => gameObject;
    public Collider GetCollider() => GetComponent<Collider>();
}
