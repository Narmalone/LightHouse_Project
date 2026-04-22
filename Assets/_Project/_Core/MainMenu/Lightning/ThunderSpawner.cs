using UnityEngine;

public class ThunderSpawner : MonoBehaviour
{
    public enum Mode
    {
        RealPosition,
        ClampedDistance,
        AroundListener
    }

    public Mode mode = Mode.ClampedDistance;

    public Transform listener;

    [Header("Clamp Distance")]
    public float minDistance = 300f;
    public float maxDistance = 1500f;

    public Vector2 randomRadius = new Vector2(200f, 800f);

    public Vector3 GetThunderPosition(Vector3 lightningPos)
    {
        if (!listener)
            listener = Camera.main.transform;

        switch (mode)
        {
            case Mode.RealPosition:
                return lightningPos;

            case Mode.ClampedDistance:
                return ClampPosition(lightningPos);

            case Mode.AroundListener:
                return RandomAroundListener();

            default:
                return lightningPos;
        }
    }

    Vector3 ClampPosition(Vector3 pos)
    {
        Vector3 dir = (pos - listener.position).normalized;
        float dist = Vector3.Distance(listener.position, pos);

        dist = Mathf.Clamp(dist, minDistance, maxDistance);

        return listener.position + dir * dist;
    }

    Vector3 RandomAroundListener()
    {
        float radius = Random.Range(randomRadius.x, randomRadius.y);
        float angle = Random.Range(0f, Mathf.PI * 2f);

        Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
        return listener.position + offset;
    }
}