using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogWindowController : MonoBehaviour
{
    public TextMeshProUGUI ExpeditorNameText;
    public TextMeshProUGUI MailObjectText;
    public TextMeshProUGUI MailBodyText;
    [SerializeField] private AutoSizeText _autoSizeText;

    public void UpdateMail(Mail obj)
    {
        ExpeditorNameText.text = "Exp: " + obj.ExpeditorName;
        MailObjectText.text = "Object: " + obj.EmailObject;
        MailBodyText.text = obj.EmailContent;
        _autoSizeText.AdjustTextSize();
    }
}
