using UnityEngine;
using KinematicCharacterController;

[RequireComponent(typeof(Rigidbody))]
public class MoverFollower : MonoBehaviour
{
    private PhysicsMover _currentMover;

    // Hauteur de raycast pour détecter un sol sous l’objet
    [SerializeField] private float _checkDistance = 0.2f;
    [SerializeField] private LayerMask _groundMask = ~0;

    private void FixedUpdate()
    {
        // 1️⃣ Vérifie s’il y a un PhysicsMover juste en dessous
        if (_currentMover == null &&  this.transform.parent == null && Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, _checkDistance, _groundMask))
        {
            _currentMover = hit.collider.GetComponentInParent<PhysicsMover>();
            this.transform.SetParent(_currentMover.transform);
        }
        else
        {
            _currentMover = null;
        }
    }
}
