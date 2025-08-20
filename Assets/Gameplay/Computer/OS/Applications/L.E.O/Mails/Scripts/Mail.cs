using UnityEngine;

namespace LightHouse.Game.Computer.LEO.Mails
{
    [System.Serializable]
    public class MailDatas
    {
        public string ExpeditorName;
        public string MailObject;
        public byte ArrivalDay;
        public float ArrivalTime;
        public string MailMessage;
        public MailAttachedFile[] Files; 
    }

}
