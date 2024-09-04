using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MailController : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI ExpeditorNameText;
    public TextMeshProUGUI ObjectDescriptionText;
    public TextMeshProUGUI ArrivalDateText;

    public MPUIKIT.MPImageBasic background_1;
    public MPUIKIT.MPImageBasic background_2;

    public Mail MailDatas;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("test");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        background_1.FallOffDistance = 0.003f;
        background_2.FallOffDistance = 0.003f;
        CursorManager.Instance.SetCursor(CursorType.ComputerClick);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        background_1.FallOffDistance = 0f;
        background_2.FallOffDistance = 0f;
        CursorManager.Instance.SetCursor(CursorType.ComputerDefault);
    }

    public void SetMail(Mail mail)
    {
        MailDatas = mail;
    }

    public void SetMail(string expeditorName, string mailObject, string arrivalTime, string mailContent)
    {
        MailDatas = new Mail(expeditorName, mailObject, arrivalTime, mailContent);
    }


    public void UpdateTextsFromMail(Mail mail)
    {
        ExpeditorNameText.text = mail.ExpeditorName;
        ObjectDescriptionText.text = mail.EmailObject;
        ArrivalDateText.text = mail.ArrivalDate;
    }
}
