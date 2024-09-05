using MPUIKIT;
using TMPro;
using UnityEngine;

public class AtmosphericController : MonoBehaviour
{
    public TextMeshProUGUI EnterAtmosphericPressureTxt;
    public TextMeshProUGUI AtmosphericPressureBarTxt;
    public TMP_InputField AtmosphericPressureIPF;

    public MPImageBasic GridBackground;
    public UiGridRenderer GridRenderer;
    public UiLineRenderer LineRenderer;

    public TextMeshProUGUI variationTxt;

    public TextMeshProUGUI minScaleTxt;
    public TextMeshProUGUI midScaleTxt;
    public TextMeshProUGUI maxScaleTxt;

    private WeatherManager _weatherManagerInstance;

    private void Start()
    {
        if(WeatherManager.Instance != null)
        {
            _weatherManagerInstance = WeatherManager.Instance;
        }
    }

    public void AddPointToRender(float pressure)
    {
        // Normaliser la pression atmosphérique entre 0 et 1 (étant donné que la plage est entre 950 et 1100)
        float normalizedPressure = 0f;
        float differencial = _weatherManagerInstance.MaxAtmosphericPressure - _weatherManagerInstance.MinAtmosphericPressure;
        if (_weatherManagerInstance != null)
        {
            normalizedPressure = (pressure - _weatherManagerInstance.MinAtmosphericPressure) / differencial;
        }
        else
        {
            normalizedPressure = (pressure - 950f) / 150f;
        }

        // Calculer la position Y en mappant la valeur normalisée à la hauteur de la grille
        float positionY = normalizedPressure * LineRenderer.gridSize.y;

        // Vérifier si le nombre de points dépasse la largeur de la grille (gridSize.x)
        if (LineRenderer.points.Count >= LineRenderer.gridSize.x)
        {
            // Retirer le premier point (le plus vieux)
            LineRenderer.points.RemoveAt(0);

            // Décaler tous les autres points vers la gauche sur l'axe X
            for (int i = 0; i < LineRenderer.points.Count; i++)
            {
                Vector2 point = LineRenderer.points[i];
                point.x -= 1;  // Déplacer chaque point d'une unité à gauche
                LineRenderer.points[i] = point;  // Mettre à jour la liste avec la nouvelle position
            }
        }

        // La position X est toujours égale au dernier index des points existants (après suppression et décalage)
        float positionX = LineRenderer.points.Count;

        // Ajouter le nouveau point à la liste
        LineRenderer.points.Add(new Vector2(positionX, positionY));

        // Met à jour les vertices du rendu
        LineRenderer.SetVerticesDirty();
    }
}
