using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MailController : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI ExpeditorNameText;
    public TextMeshProUGUI ObjectDescriptionText;
    public TextMeshProUGUI ArrivalDateText;

    [SerializeField] private CustomEvent_Mail _onMailSelectedChanged;

    public MPUIKIT.MPImageBasic background_1;
    public MPUIKIT.MPImageBasic background_2;

    private bool _isSelected;

    [SerializeField] private Color _selectedColor;

    public Mail MailDatas;

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
        if (obj.ExpeditorName != this.MailDatas.ExpeditorName || !_isSelected) return;

        background_1.color = Color.white;
        background_2.color = Color.white;
        _isSelected = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isSelected) return;
        _onMailSelectedChanged?.Raise(MailDatas);
        _isSelected = true;
        background_1.color = _selectedColor;
        background_2.color = _selectedColor;
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
        UpdateTextsFromMail(MailDatas);
    }

    public void SetMail(string expeditorName, string mailObject, string arrivalTime, string mailContent)
    {
        MailDatas = new Mail(expeditorName, mailObject, arrivalTime, mailContent);
        UpdateTextsFromMail(MailDatas);
    }


    public void UpdateTextsFromMail(Mail mail)
    {
        ExpeditorNameText.text = mail.ExpeditorName;
        ObjectDescriptionText.text = mail.EmailObject;
        ArrivalDateText.text = mail.ArrivalDate;
    }
}
