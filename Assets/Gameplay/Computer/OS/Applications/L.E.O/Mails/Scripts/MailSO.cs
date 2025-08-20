using UnityEngine;
using UnityEngine.Localization;

namespace LightHouse.Game.Computer.LEO.Mails
{
    [CreateAssetMenu(fileName = "Mail_", menuName = "LightHouse/Computer/LEO/Mails/New Mail")]
    public class MailSO : ScriptableObject
    {
        public LocalizedString ExpeditorName;
        public string MailObject;
        public byte ArrivalDay;
        public float ArrivalTime;
        public string MailMessage;
        public MailAttachedFile[] Files;

    }
}
