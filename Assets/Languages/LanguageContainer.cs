using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanguageContainer : MonoBehaviour
{
    public KeyWordLanguage AutomaticKeyword;
    public List<MultiLanguage> Languages = new List<MultiLanguage>();

    private void OnValidate()
    {
        if(AutomaticKeyword != null)
        {
            Languages = AutomaticKeyword.Languages;
        }
    }
}
