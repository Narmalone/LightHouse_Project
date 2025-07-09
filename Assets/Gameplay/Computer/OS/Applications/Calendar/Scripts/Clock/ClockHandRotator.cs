using LightHouse.Game.DayNightSystem;
using UnityEngine;

public class ClockHandRotator : MonoBehaviour
{
    #region Serialized Fields

    [Header("Aiguilles")]
    public Transform _handHour;   // Petite aiguille (heures)
    public Transform _handMinits; // Grande aiguille (minutes)

    #endregion

    #region Unity Methods

    private void LateUpdate()
    {
        UpdateRotation();
    }

    #endregion

    #region Clock Logic

    /// <summary>
    /// Met à jour la rotation des aiguilles selon l'heure (mode debug ou temps réel).
    /// </summary>
    public void UpdateRotation()
    {
        float hourOn12 = TimeHandlerData.CurrentTime % 12f;
        float minutes = (hourOn12 - Mathf.Floor(hourOn12)) * 60f;
        float hourAngle = -hourOn12 * 30f;   // 360° ÷ 12
        float minuteAngle = -minutes * 6f;   // 360° ÷ 60

        if (_handHour != null)
            _handHour.localRotation = Quaternion.Euler(0f, 0f, hourAngle);

        if (_handMinits != null)
            _handMinits.localRotation = Quaternion.Euler(0f, 0f, minuteAngle);
    }

    #endregion
}
