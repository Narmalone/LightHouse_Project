using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Messagerie : ContentWindow
{
    [SerializeField] private MailController _mailPrefab;
    [SerializeField] private ScrollWindowController _mailScrollWindow;
    [SerializeField] public DialogWindowController _dialogWindow;
    [SerializeField] private CustomEvent_Mail _onMailSelectedChanged;
    public TextMeshProUGUI ReceptionBoxText;
    public TextMeshProUGUI UnreadsText;
    public TextMeshProUGUI NowPlayingText;
    public TextMeshProUGUI ReadsText;

    public LanguageText[] AllTextsLanguages;

    public List<MailController> Controllers;

    public Mail CurrentOpenedMail;

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
        CurrentOpenedMail = obj;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            GenerateMail("Coucou", "Salutation_" + Random.Range(0, 300), $"J{Random.Range(0, 31)}-{Random.Range(0, 23)}h{Random.Range(1, 60)}", $"Je m'appelle kaka {Random.Range(0, 2000)}");
        }
    }

    public void GenerateMail(Mail mail)
    {
        MailController newMail = Instantiate(_mailPrefab, _mailScrollWindow.Content);
        newMail.SetMail(mail);
        Controllers.Add(newMail);
        if (Controllers.Count >= 10)
        {
            _mailScrollWindow.UpdateContentTransform();
        }
    }

    public void GenerateMail(string expeditorName, string mailObject, string arrivalTime, string mailContent)
    {
        MailController newMail = Instantiate(_mailPrefab, _mailScrollWindow.Content);
        newMail.SetMail(expeditorName, mailObject, arrivalTime, mailContent);
        Controllers.Add(newMail);
        if(Controllers.Count >= 10)
        {
            _mailScrollWindow.UpdateContentTransform();
        }
    }

    //Specific Mails ?
    //Very old mails ?
    public void DeleteMail()
    {

    }
}
