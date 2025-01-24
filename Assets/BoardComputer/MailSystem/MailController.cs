using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public enum MailState
{
    Unread,
    NowReading,
    Read
}

public class MailController : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header(" -- TEXTS -- ")]
    public TextMeshProUGUI ExpeditorNameText;
    public TextMeshProUGUI ObjectDescriptionText;
    public TextMeshProUGUI ArrivalDateText;

    [Header(" -- EVENT --")]
    [SerializeField] private CustomEvent_Mail _onMailSelectedChanged;

    [Header(" -- BACKGROUNDS -- ")]
    public MPUIKIT.MPImageBasic Background_ExpeditorName;
    public MPUIKIT.MPImageBasic Background_Body;
    public MPUIKIT.MPImageBasic Background_ReadState;

    [Header(" -- COLORS -- ")]
    [SerializeField] private Color _selectedColor;
    [SerializeField] private Color _unselectedColor = Color.white;

    [SerializeField] private Color _unreadColor;
    [SerializeField] private Color _readingColor;
    [SerializeField] private Color _readedColor;

    [Header(" DEBUG / DO NOT SET")]
    public Mail MailDatas = new Mail();
    public MailState CurrentMailState;

    private bool _isSelected;

    private void Awake()
    {
        _onMailSelectedChanged.handle += _onMailSelectedChanged_handle;
        CurrentMailState = MailState.Unread;
    }

    private void OnDestroy()
    {
        _onMailSelectedChanged.handle -= _onMailSelectedChanged_handle;
    }

    public void SetMailState(MailState mailState)
    {
        switch (mailState)
        {
            case MailState.Unread:
                Background_ReadState.color = _unreadColor;
                break;
            case MailState.NowReading:
                Background_ReadState.color = _readingColor;
                break;
            case MailState.Read:
                Background_ReadState.color = _readedColor;
                break;
        }
        CurrentMailState = mailState;
    }

    private void _onMailSelectedChanged_handle(Mail obj)
    {
        if (obj.ExpeditorName != this.MailDatas.ExpeditorName || !_isSelected) return;

        Background_ExpeditorName.color = Color.white;
        Background_Body.color = Color.white;
        _isSelected = false;
        SetMailState(MailState.Read);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isSelected) return;
        _onMailSelectedChanged?.Raise(MailDatas);
        _isSelected = true;
        Background_ExpeditorName.color = _selectedColor;
        Background_Body.color = _selectedColor;
        SetMailState(MailState.NowReading);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_isSelected) return;
        Background_ExpeditorName.color = _selectedColor;
        Background_Body.color = _selectedColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_isSelected) return;
        Background_ExpeditorName.color = Color.white;
        Background_Body.color = Color.white;
    }

    public void SetMail(Mail mail)
    {
        MailDatas = mail;
        UpdateTextsFromMail(MailDatas);
    }

    public void SetColors(Color unread, Color reading, Color readed)
    {
        _unreadColor = unread;
        _readingColor = reading;
        _readedColor = readed;
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
