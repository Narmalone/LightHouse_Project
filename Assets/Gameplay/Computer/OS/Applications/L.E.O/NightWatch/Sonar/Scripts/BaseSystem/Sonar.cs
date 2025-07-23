using UnityEngine;

namespace LightHouse.Game.Computer.NightWatch.Sonar
{
    public class Sonar : MonoBehaviour
    {
        public bool EnableSonarDetector = true;
        public float DetectionRange = 100f;

        private void Awake()
        {
            SonarHandlerData.Sonar = this;
        }

        private void Update()
        {
            if (!EnableSonarDetector) return;
            float rayonSqr = DetectionRange * DetectionRange;
            Vector3 centre = transform.position;

            foreach (var sonarItem in SonarHandlerData.SonarItems)
            {
                if ((sonarItem.Position - centre).sqrMagnitude <= rayonSqr)
                {
                    // ✔️ Détection : le bateau est dans la sphère
                    if (!sonarItem.IsDetectedBySonar)
                    {
                        Debug.Log("Nouvel Objet détecté : " + sonarItem.Name);
                        sonarItem.IsDetectedBySonar = true;
                        sonarItem.UniqueID = SonarIDAllocator.AllocateID();
                    }
                }
                else
                {
                    if (sonarItem.IsDetectedBySonar)
                    {
                        sonarItem.IsDetectedBySonar = false;
                        SonarIDAllocator.ReleaseID(sonarItem.UniqueID);
                    }
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
