using LightHouse.Game.Computer.LEO.Mails;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_MailPopup : MonoBehaviour
{
    [SerializeField] private Button _receiptButton;
    [SerializeField] private TextMeshProUGUI _expeditorText;
    [SerializeField] private TextMeshProUGUI _objectText;
    [SerializeField] private TextMeshProUGUI _bodyText;
    [SerializeField] private TextMeshProUGUI _arrivalDateText;

    public event Action OnMailReceiptCliqued;
    public MailDatas MyDatas { get; private set; }

    private void Awake()
    {
        _receiptButton.onClick.AddListener(OnReceiptCliqued);
    }

    private void OnDestroy()
    {
        _receiptButton.onClick.RemoveListener(OnReceiptCliqued);
    }

    private void OnReceiptCliqued()
    {
        OnMailReceiptCliqued?.Invoke();
    }

    public void Initialize(MailDatas datas)
    {
        MyDatas = datas;
        _objectText.text = datas.MailObject;
        _bodyText.text = datas.MailMessage;
        _expeditorText.text = datas.ExpeditorName;
        _arrivalDateText.text = datas.GetDate().ToUpper();
    }
}
