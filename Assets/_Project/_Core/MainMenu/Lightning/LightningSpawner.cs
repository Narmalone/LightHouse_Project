using UnityEngine;

public class LightningSpawner : MonoBehaviour
{
    public Transform center;

    public float minRadius = 800f;
    public float maxRadius = 2500f;
    public float height = 1200f;

    public Vector3 GetPosition()
    {
        if (!center)
            center = Camera.main.transform;

        float radius = Random.Range(minRadius, maxRadius);
        float angle = Random.Range(0f, Mathf.PI * 2f);

        Vector3 pos = center.position +
                      new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;

        pos.y = height;

        return pos;
    }
}