using UnityEngine;

public class CameraMenuController : MonoBehaviour
{
    [Header("Breathing")]
    public float amplitude = 0.015f;
    public float frequency = 1.0f;

    [Header("Noise")]
    public float noiseSpeed = 0.5f;
    public float noiseAmount = 0.5f;

    [Header("Rotation")]
    public float rotX = 0.3f;
    public float rotY = 0.2f;

    private Vector3 startPos;
    private Quaternion startRot;

    void Start()
    {
        startPos = transform.localPosition;
        startRot = transform.localRotation;
    }

    void Update()
    {
        float time = Time.time;

        // Bruit organique (irrégulier)
        float noise = Mathf.PerlinNoise(time * noiseSpeed, 0f) * noiseAmount;

        // Respiration principale (pas parfaite)
        float breath = Mathf.Sin(time * frequency + noise) * amplitude;

        // Petit mouvement secondaire (désynchronisé)
        float breath2 = Mathf.Sin(time * (frequency * 0.5f)) * (amplitude * 0.3f);

        // Position (pas juste Y !)
        Vector3 offset = new Vector3(
            Mathf.Sin(time * 0.3f) * 0.002f,   // léger drift X
            breath + breath2,                  // respiration Y
            Mathf.Cos(time * 0.2f) * 0.002f    // léger drift Z
        );

        transform.localPosition = startPos + offset;

        // Rotation subtile et organique
        float rotOffsetX = Mathf.Sin(time * frequency + noise) * rotX;
        float rotOffsetY = Mathf.Cos(time * 0.7f) * rotY;

        transform.localRotation = startRot * Quaternion.Euler(rotOffsetX, rotOffsetY, 0);
    }
}