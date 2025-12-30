using LightHouse.Game.DayNightSystem;

namespace LightHouse.Game.Computer.LEO.Mails
{
    [System.Serializable]
    public class MailDatas
    {
        public string ExpeditorName;
        public string MailObject;
        public int ArrivalDay;
        public float ArrivalTime;
        public string MailMessage;
        public MailAttachedFile[] Files; 

        public string GetDate()
        {
            return TimeUtility.FormatDate(ArrivalDay, ArrivalTime);
        }
    }

}
