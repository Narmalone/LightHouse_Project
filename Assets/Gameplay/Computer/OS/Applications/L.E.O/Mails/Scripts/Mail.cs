using UnityEngine;

namespace LightHouse.Game.Computer.MailSystem
{
    [System.Serializable]
    public class Mail
    {
        public string ExpeditorName;
        public string MailObject;
        public byte ArrivalDay;
        public float ArrivalTime;
        public string MailMessage;
        public MailAttachedFile[] Files; 
    }

}
