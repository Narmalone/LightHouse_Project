using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Event_", menuName = "CustomEvent/Event_Mail", order = 0)]
public class CustomEvent_Mail : ScriptableObject
{
    public event Action<Mail> handle;
    public void Raise(Mail mail)
    {
        if(mail == null)
        {
            Raise(mail.ExpeditorName, mail.EmailObject, mail.ArrivalDate, mail.EmailContent);
        }
        else
        {
            handle?.Invoke(mail);
        }
    }

    public void Raise(string expeName, string mailObj, string arrivalData, string mailContent)
    {
        handle?.Invoke(new Mail(expeName, mailObj, arrivalData, mailContent));
    }
}
