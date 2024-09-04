using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogWindowController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _expeditorText;
    [SerializeField] private TextMeshProUGUI _mailObjectText;
    [SerializeField] private TextMeshProUGUI _mailBodyText;
    [SerializeField] private AutoSizeText _autoSizeText;

    [SerializeField] private CustomEvent_Mail _onMailSelectedChanged;

    private void Awake()
    {
        _onMailSelectedChanged.handle += _onMailSelectedChanged_handle;
    }

    private void OnDestroy()
    {
        _onMailSelectedChanged.handle -= _onMailSelectedChanged_handle;
    }

    private void _onMailSelectedChanged_handle(Mail obj)
    {
        _expeditorText.text = "Exp: " + obj.ExpeditorName;
        _mailObjectText.text = "Object: " + obj.EmailObject;
        _mailBodyText.text = obj.EmailContent;
        _autoSizeText.AdjustTextSize();
    }
}
