using UnityEngine;

namespace LightHouse.Game.Computer.NightWatch.Sonar
{
    public class Sonar : MonoBehaviour
    {
        public bool Continuous = true;
        public float DetectionRange = 100f;

        private void Update()
        {
            if (!Continuous) return;
            float rayonSqr = DetectionRange * DetectionRange;
            Vector3 centre = transform.position;

            foreach (var bateau in SonarManager.SonarItems)
            {
                if ((bateau.Position - centre).sqrMagnitude <= rayonSqr)
                {
                    // ✔️ Détection : le bateau est dans la sphère
                    Debug.Log("Bateau détecté : " + bateau.Name);
                    bateau.IsDetectedBySonar = true;
                }
                else
                {
                    if (bateau.IsDetectedBySonar)
                        bateau.IsDetectedBySonar = false;
                }
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            //Gizmos.color = new Color(0f, 1f, 0f, 0.25f); // Vert transparent
            //Gizmos.DrawSphere(transform.position, rayonDetection);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, DetectionRange);
        }
#endif
    }

}
