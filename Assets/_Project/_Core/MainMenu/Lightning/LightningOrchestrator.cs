using System.Collections;
using UnityEngine;

public class LightningOrchestrator : MonoBehaviour
{
    [SerializeField] private LightningSpawner lightningSpawner;
    [SerializeField] private LightningFlashEffect flash;
    [SerializeField] private ThunderEffect thunder;

    [Range(0f, 1f)] public float stormLevel = 1f;
    public Vector2 intervalRange = new Vector2(6f, 20f);

    private Coroutine loop;

    private void Start()
    {
        StartStorm();
    }

    public void StartStorm()
    {
        if (loop == null)
            loop = StartCoroutine(Run());
    }

    public void StopStorm()
    {
        if (loop != null)
        {
            StopCoroutine(loop);
            loop = null;
        }
    }

    private IEnumerator Run()
    {
        while (true)
        {
            float delay = Random.Range(intervalRange.x, intervalRange.y) * Mathf.Lerp(2f, 1f, stormLevel);
            yield return new WaitForSeconds(delay);

            Vector3 pos = lightningSpawner.GetPosition();

            flash.PlayFlash(pos);

            if (thunder != null)
                thunder.PlayThunder(pos);
        }
    }
}