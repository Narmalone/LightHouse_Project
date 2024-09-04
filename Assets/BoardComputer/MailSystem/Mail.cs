[System.Serializable]
public struct Mail
{
    public string ExpeditorName;
    public string EmailObject;
    public string ArrivalDate;
    public string EmailContent;

    public Mail(string expeName, string mailObj, string arrivalDate, string emailContent)
    {
        ExpeditorName = expeName;
        EmailObject = mailObj;
        ArrivalDate = arrivalDate;
        EmailContent = emailContent;
    }
}
