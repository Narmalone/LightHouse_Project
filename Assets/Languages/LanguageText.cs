using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LanguageText : MonoBehaviour
{
    //store language event
    [SerializeField] private CustomEvent_Int _onLanguageChanged;
    public TextMeshProUGUI Text;
    public List<MultiLanguage> mLanguages = new List<MultiLanguage>();

    private void Awake()
    {
        _onLanguageChanged.handle += _onLanguageChanged_handle;
    }

    private void OnDestroy()
    {
        _onLanguageChanged.handle -= _onLanguageChanged_handle;
    }

    private void _onLanguageChanged_handle(int obj)
    {
        Text.text = FindWithLanguage((Languages)obj).Value;
    }

    public MultiLanguage FindWithLanguage(Languages target)
    {
        return mLanguages.Find(x => x.Language == target);
    }

    private void OnValidate()
    {
        if(GetComponent<TextMeshProUGUI>() != null)
        {
            Text = GetComponent<TextMeshProUGUI>();
        }
    }
}
