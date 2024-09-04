using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Messagerie : MonoBehaviour
{
    [SerializeField] private MailController _mailPrefab;
    [SerializeField] private ScrollWindowController _mailScrollWindow;
    public List<MailController> Controllers;

    public Mail CurrentOpenedMail;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            GenerateMail("Coucou", "Salutation_" + Random.Range(0, 300), $"J-{Random.Range(0, 31)}: h{Random.Range(0, 23)}", $"Je m'appelle kaka {Random.Range(0, 2000)}");
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
