using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Game.Computer.LEO.Mails
{
    public enum E_MailState
    {
        Unread,
        Read
    }

    public class UI_MailElement : MonoBehaviour
    {
        [SerializeField] private E_MailState _state = E_MailState.Unread;
        [SerializeField] private MailDatas _datas = null;
        [SerializeField] private Image _mailStateImage;
        [SerializeField] private Image _background;
        [SerializeField] private TextMeshProUGUI _expeditorText;
        [SerializeField] private TextMeshProUGUI _objectText;
        [SerializeField] private TextMeshProUGUI _arrivalDateText;

        public void Initialize(MailDatas datas)
        {
            _expeditorText.text = _datas.ExpeditorName;
            _objectText.text = _datas.MailObject;
            //_arrivalDateText.text = ;
        }

        public void SetIcon(Sprite icon)
        {
            _mailStateImage.sprite = icon;
        }

        public void Open()
        {

        }

        public void SetState(E_MailState nextState)
        {
            switch(nextState)
            {
                case E_MailState.Unread:
                    break;
                case E_MailState.Read:
                    break;
            }
        }
    }
}
