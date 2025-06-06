using System.Collections.Generic;
using UnityEngine;

namespace LightHouse.Items
{
    public class PhysicsDropQueueSystem : MonoBehaviour
    {
        private struct DropData
        {
            public Rigidbody Rigidbody;
            public Vector3 Position;
            public Quaternion Rotation;
            public Vector3 ForceDirection;
            public float Force;
        }

        private static readonly List<DropData> _dropQueue = new();

        public static void QueueDrop(Rigidbody rb, Vector3 position, Quaternion rotation, Vector3 direction, float force)
        {
            _dropQueue.Add(new DropData
            {
                Rigidbody = rb,
                Position = position,
                Rotation = rotation,
                ForceDirection = direction,
                Force = force
            });
        }

        private void FixedUpdate()
        {
            if (_dropQueue.Count <= 0) return;
            foreach (var drop in _dropQueue)
            {
                if (drop.Rigidbody == null) continue;
                if(drop.Force == 0f)
                {
                    drop.Rigidbody.linearVelocity = Vector3.zero;
                    drop.Rigidbody.angularVelocity = Vector3.zero;
                }
                drop.Rigidbody.position = drop.Position;
                drop.Rigidbody.transform.position = drop.Position;
                drop.Rigidbody.MovePosition(drop.Position);
                //drop.Rigidbody.MoveRotation(drop.Rotation);
                drop.Rigidbody.AddForce(drop.ForceDirection * drop.Force, ForceMode.Impulse);
            }

            _dropQueue.Clear(); // Toujours vider après traitement
        }
    }

}
