using System;
using TMPro;
using UnityEngine;

[System.Serializable]
public struct BeaufortScale
{
    public int Level;
    public float MinWindSpeed;
    public float MaxWindSpeed;
    public string Title;
    [TextArea(1, 5)] public string Description;
}

public class UI_WindWindowController : MonoBehaviour
{
    [SerializeField] private TMP_InputField IPF_windSpeed;
    [SerializeField] private TextMeshProUGUI _beaufortLevel;
    public BeaufortScale[] fces;

    private void Awake()
    {
        IPF_windSpeed.onValueChanged.AddListener(OnWindSpeedChanged);
    }

    private void OnDestroy()
    {
        IPF_windSpeed.onValueChanged.RemoveListener(OnWindSpeedChanged);
    }

    private void OnWindSpeedChanged(string arg0)
    {
        if (float.TryParse(arg0, out float windSpeed))
        {
            Debug.Log($"Wind Speed = {windSpeed}");

            BeaufortScale? matched = FindMatchingBeaufortLevel(windSpeed);
            if (matched.HasValue)
            {
                var scale = matched.Value;
                //Debug.Log($"FCE {scale.Level}: {scale.Title} - {scale.Description}");
                //_beaufortLevel.text = $"FCE {scale.Level}: {scale.Title} - {scale.Description}";
                _beaufortLevel.text = $"FCE {scale.Level}: {scale.Title}";
                // Ici tu peux mettre à jour ton UI, par exemple afficher le titre et la description
            }
            else
            {
                Debug.LogWarning("Vitesse hors de l’échelle de Beaufort !");
            }
        }
        else
        {
            Debug.LogWarning($"Valeur non valide : {arg0}");
        }
    }

    private BeaufortScale? FindMatchingBeaufortLevel(float windSpeed)
    {
        foreach (var fce in fces)
        {
            if (windSpeed >= fce.MinWindSpeed && windSpeed <= fce.MaxWindSpeed)
            {
                return fce;
            }
        }

        return null; // Aucun niveau trouvé
    }


}
