using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum Languages
{
    EN,
    FR,
    SPA,
    DE,
}

public class LanguageManager : MonoBehaviour
{
    public Languages CurrentLanguage;
    [SerializeField] private CustomEvent_Int _onLanguageChanged;
    [SerializeField] private KeyWordLanguage[] _keywords;
    //stocker tous les keywords et pour les initializer en fonction de la langues sauvegardés par une partie antérieure
    //du joueur

    private void Awake()
    {
        ChangeLanguage(Languages.EN);
    }

    public void ChangeLanguage(Languages toLanguage)
    {
        CurrentLanguage = toLanguage;
        foreach (var k in _keywords)
        {
            k.SetValueByLanguage(toLanguage);
        }
        _onLanguageChanged?.Raise((int)toLanguage);
    }

}

[System.Serializable]
public struct MultiLanguage
{
    public Languages Language;
    public string Value;
}
