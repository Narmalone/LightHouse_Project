using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QualityAtmosphereController : MonoBehaviour
{
    [SerializeField] private Slider _humiditySlider;
    [SerializeField] private TextMeshProUGUI _humiditySelectedText;

    public UiLineRenderer rend;

    private void Awake()
    {
        _humiditySlider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    public void AddPointToRender(float pressure)
    {
        // Normaliser la pression atmosphérique entre 0 et 1 (étant donné que la plage est entre 950 et 1100)
        float normalizedPressure = (pressure - 950f) / 150f;

        // Calculer la position Y en mappant la valeur normalisée à la hauteur de la grille
        float positionY = normalizedPressure * rend.gridSize.y;

        // Vérifier si le nombre de points dépasse la largeur de la grille (gridSize.x)
        if (rend.points.Count >= rend.gridSize.x)
        {
            // Retirer le premier point (le plus vieux)
            rend.points.RemoveAt(0);

            // Décaler tous les autres points vers la gauche sur l'axe X
            for (int i = 0; i < rend.points.Count; i++)
            {
                Vector2 point = rend.points[i];
                point.x -= 1;  // Déplacer chaque point d'une unité à gauche
                rend.points[i] = point;  // Mettre à jour la liste avec la nouvelle position
            }
        }

        // La position X est toujours égale au dernier index des points existants (après suppression et décalage)
        float positionX = rend.points.Count;

        // Ajouter le nouveau point à la liste
        rend.points.Add(new Vector2(positionX, positionY));

        // Met à jour les vertices du rendu
        rend.SetVerticesDirty();
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            AddPointToRender(Random.Range(950f, 1100f));
        }
    }



    private void Start()
    {
        UpdateTextBySliderValue();

        /*     AddPointToRender(950f);
             AddPointToRender(1000f);
             AddPointToRender(1050f);
             AddPointToRender(1100f);*/
        AddPointToRender(Random.Range(950f, 1100f));
        AddPointToRender(Random.Range(950f, 1100f));
        AddPointToRender(Random.Range(950f, 1100f));
        AddPointToRender(Random.Range(950f, 1100f));
        AddPointToRender(Random.Range(950f, 1100f));
        AddPointToRender(Random.Range(950f, 1100f));
        AddPointToRender(Random.Range(950f, 1100f));
        AddPointToRender(Random.Range(950f, 1100f));
        AddPointToRender(Random.Range(950f, 1100f));
        AddPointToRender(Random.Range(950f, 1100f));
        AddPointToRender(Random.Range(950f, 1100f));
        AddPointToRender(Random.Range(950f, 1100f));
        AddPointToRender(Random.Range(950f, 1100f));
        AddPointToRender(Random.Range(950f, 1100f));
        AddPointToRender(Random.Range(950f, 1100f));
    }

    private void OnDestroy()
    {
        _humiditySlider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }

    private void OnSliderValueChanged(float arg0)
    {
        float value = arg0 * 100;
        string valeurArrondie = string.Format("{0:F2}", value);
        _humiditySelectedText.text = valeurArrondie + "%";
    }

    private void UpdateTextBySliderValue()
    {
        float value = _humiditySlider.value * 100;
        string valeurArrondie = string.Format("{0:F2}", value);
        _humiditySelectedText.text = valeurArrondie + "%";
    }
}
