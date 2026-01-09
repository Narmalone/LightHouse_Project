using UnityEngine;
using KinematicCharacterController;
using System;

[RequireComponent(typeof(Rigidbody))]
public class MoverFollower : MonoBehaviour
{
    public event Action OnMoverAttached;
    private PhysicsMover _currentMover;

    // Hauteur de raycast pour détecter un sol sous l’objet
    [SerializeField] private float _checkDistance = 0.2f;
    [SerializeField] private LayerMask _groundMask = ~0;

    public void Config(LayerMask mask, float checkDistance)
    {
        _groundMask = mask;
        _checkDistance = checkDistance;
    }

    private void FixedUpdate()
    {
        // 1️⃣ Vérifie s’il y a un PhysicsMover juste en dessous
        if (_currentMover == null &&  this.transform.parent == null && Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, _checkDistance, _groundMask))
        {
            _currentMover = hit.collider.GetComponentInParent<PhysicsMover>();
            this.transform.SetParent(_currentMover.transform);
            OnMoverAttached?.Invoke();
        }
        else
        {
            _currentMover = null;
        }
    }
}
