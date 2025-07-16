using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResolutionSettings : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _displayText;

    Resolution[] _resolutions;

    private int _indexResolution;

    List<string> _resolutionsNames;

    void Start()
    {
        // toutes les résolutions 
        _resolutions = Screen.resolutions;

        // liste qui va stocker les noms de chaque résolutions 
        _resolutionsNames = new List<string>();

        SetOptionsNames();

        SetDisplayText();
    }

    void SetOptionsNames()
    {
        for (int i = 0; i < _resolutions.Length; i++)
        {
            // concaténe la largeur et la hauteur de la résolution
            string name = _resolutions[i].width + " X " + _resolutions[i].height;

            // ajoute l'option à la liste d'options
            _resolutionsNames.Add(name);
        }
    }

    public void Increment()
    {
        // increment
        _indexResolution++;
         
        // clamp _indexResolution entre 0 et le nombre d'option 
        _indexResolution = Mathf.Clamp(_indexResolution, 0, _resolutionsNames.Count - 1);

        // met à jour le text
        SetDisplayText();

    }

    public void Decrement()
    {
        // décrement
        _indexResolution--;

        // clamp _indexResolution entre 0 et le nombre d'option 
        _indexResolution = Mathf.Clamp(_indexResolution, 0, _resolutionsNames.Count - 1);

        // met à jour le text
        SetDisplayText();
    }

    void SetDisplayText()
    {
        if (_indexResolution >= 0 && _indexResolution < _resolutionsNames.Count)
        {
            _displayText.text = _resolutionsNames[_indexResolution];
        }
    }
}
