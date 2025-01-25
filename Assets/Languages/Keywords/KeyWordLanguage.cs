using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Keyword_", menuName = "Languages/NewKeyword")]
//Scriptables objects marchent bien car ce sont des mots clés en fonctions des langues et qu'on a le jeu
//que dans une seule langue en même temps
public class KeyWordLanguage : ScriptableObject
{
    public string KeywordName;
    public List<MultiLanguage> Languages = new List<MultiLanguage>();

    [HideInInspector] public string CurrentValue;

    public string SetValueByLanguage(Languages language)
    {
        CurrentValue = FindWithLanguage(language).Value;
        return CurrentValue;
    }

    public MultiLanguage FindWithLanguage(Languages target)
    {
        return Languages.Find(x => x.Language == target);
    }
}
