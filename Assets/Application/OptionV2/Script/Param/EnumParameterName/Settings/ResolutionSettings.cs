using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResolutionSettings : MonoBehaviour, IConfigurable
{
    [SerializeField] TextMeshProUGUI _displayText;

    Resolution[] _resolutions;
    List<string> _resolutionsNames;

    private int _indexResolution;
    private int _currentResolutionIndex;

    void Start()
    {
        // toutes les résolutions d'écran
        _resolutions = Screen.resolutions;

        // liste qui va stocker les noms de chaque résolutions 
        _resolutionsNames = new List<string>();

        SetOptionsNames();

        _indexResolution = _currentResolutionIndex;

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

            // vérifie si une dimension dans résolution correspond à la dimension de l'écran du joueur
            if (_resolutions[i].width == Screen.width && _resolutions[i].height == Screen.height)
            {
                // set _currentResolutionIndex à la taille de l'écran du joueur
                _currentResolutionIndex = i;
            }
        }
    }

    public void Increment()
    {
        // incremente
        _indexResolution++;
         
        // clamp _indexResolution entre 0 et le nombre d'option 
        _indexResolution = Mathf.Clamp(_indexResolution, 0, _resolutionsNames.Count - 1);

        // met à jour le text
        SetDisplayText();
    }

    public void Decrement()
    {
        // décremente
        _indexResolution--;

        // clamp _indexResolution entre 0 et le nombre d'option 
        _indexResolution = Mathf.Clamp(_indexResolution, 0, _resolutionsNames.Count - 1);

        // met à jour le text
        SetDisplayText();
    }

    void SetDisplayText()
    {
        _displayText.text = _resolutionsNames[_indexResolution];
    }

    public bool HasChanged()
    {
        return _displayText != null;
    }

    public void Apply()
    {
        throw new System.NotImplementedException();
    }

    public void Revert()
    {
        throw new System.NotImplementedException();
    }
}
