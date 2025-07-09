using LightHouse.Game.DayNightSystem;
using UnityEngine;

[ExecuteInEditMode]
public class ClockHandRotator : MonoBehaviour
{
    [Range(0f, 24f)]
    public float hour24 = 0f;
    public bool DebugMode = true;
    [Header("Aiguilles")]
    public Transform hourHand;   // Petite aiguille
    public Transform minuteHand; // Grande aiguille

    private void Update()
    {
        UpdateRotation();
    }

    public void UpdateRotation()
    {
        float hourOn12 = 0f; // Ex: 13.5 → 1.5h
        if (DebugMode)
        {
            hourOn12 = hour24 % 12f;
        }
        else
        {
            hourOn12 = TimeHandlerData.CurrentTime % 12f;
        }
        float minutes = (hourOn12 - Mathf.Floor(hourOn12)) * 60f; // décimales → minutes

        float hourAngle = -hourOn12 * 30f;    // 360° / 12
        float minuteAngle = -minutes * 6f;    // 360° / 60

        if (hourHand != null)
            hourHand.localRotation = Quaternion.Euler(0f, 0f, hourAngle);

        if (minuteHand != null)
            minuteHand.localRotation = Quaternion.Euler(0f, 0f, minuteAngle);
    }
}
