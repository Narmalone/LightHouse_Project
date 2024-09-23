using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//EVENT QUAND LE LANGUAGE CHANGE
public enum Languages
{
    EN,
    FR,
    SPA,
    DE,
}

public class LanguageManager : MonoBehaviour
{
    
}

[System.Serializable]
public struct MultiLanguage
{
    public Languages Language;
    public string Value;
}
