using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LanguageText : MonoBehaviour
{
    //store language event

    public TextMeshProUGUI Text;
    public List<MultiLanguage> mLanguages = new List<MultiLanguage>();

    private void OnValidate()
    {
        if(GetComponent<TextMeshProUGUI>() != null)
        {
            Text = GetComponent<TextMeshProUGUI>();
        }
    }
}
