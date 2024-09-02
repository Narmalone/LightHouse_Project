using UnityEngine;

public class SimpleBoatMotion : MonoBehaviour
{
    [Header("Boat Movement")]
    public float speed = 5.0f;        // Vitesse du bateau
    public float turnSpeed = 2.0f;    // Vitesse de rotation du bateau

    [Header("Boat Tilting Settings")]
    public float tiltAmplitudeX = 5.0f;  // Amplitude de l'inclinaison avant/arrière
    public float tiltAmplitudeZ = 10.0f; // Amplitude du tangage gauche/droite
    public float tiltFrequencyX = 0.5f;  // Fréquence de l'inclinaison avant/arrière
    public float tiltFrequencyZ = 0.7f;  // Fréquence du tangage gauche/droite

    private float tiltOffsetX;
    private float tiltOffsetZ;

    // Start est appelé avant la première frame
    void Start()
    {
        // Initialisation des offsets aléatoires pour les oscillations
        tiltOffsetX = Random.Range(0f, Mathf.PI * 2f);
        tiltOffsetZ = Random.Range(0f, Mathf.PI * 2f);
    }

    // Update est appelé à chaque frame
    void Update()
    {
        // Appliquer les oscillations de tangage et d'inclinaison
        ApplyBoatTilting();

        // Déplacer le bateau en avant et permettre la rotation
        ApplyBoatMovement();
    }

    // Appliquer le tangage et l'inclinaison du bateau
    private void ApplyBoatTilting()
    {
        // Oscillation sur l'axe X pour l'inclinaison avant/arrière
        float tiltX = Mathf.Sin(Time.time * tiltFrequencyX + tiltOffsetX) * tiltAmplitudeX;

        // Oscillation sur l'axe Z pour le tangage gauche/droite
        float tiltZ = Mathf.Sin(Time.time * tiltFrequencyZ + tiltOffsetZ) * tiltAmplitudeZ;

        // Appliquer les rotations sur le bateau
        transform.localRotation = Quaternion.Euler(tiltX, transform.localEulerAngles.y, tiltZ);
    }

    // Appliquer le mouvement et la rotation du bateau
    private void ApplyBoatMovement()
    {
        // Déplacer le bateau en avant
        transform.Translate(Vector3.right * speed * Time.deltaTime);

        // Tourner le bateau
        float horizontalInput = Input.GetAxis("Horizontal");  // Pour la rotation via les touches directionnelles (optionnel)
        transform.Rotate(Vector3.up, horizontalInput * turnSpeed * Time.deltaTime);
    }
}
