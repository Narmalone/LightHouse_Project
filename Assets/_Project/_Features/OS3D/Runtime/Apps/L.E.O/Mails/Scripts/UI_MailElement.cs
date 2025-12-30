using System;
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
        [SerializeField] private Sprite _unreadSprite;
        [SerializeField] private Sprite _readSprite;
        [SerializeField] private Image _background;
        [SerializeField] private UI_CustomButton _customButton;
        [SerializeField] private TextMeshProUGUI _expeditorText;
        [SerializeField] private TextMeshProUGUI _objectText;
        [SerializeField] private TextMeshProUGUI _arrivalDateText;

        [SerializeField] private Color _unreadColor;
        [SerializeField] private Color _readColor;

        public E_MailState MailState => _state;
        public event Action<MailDatas> OpenMailCliqued;

        private void Awake()
        {
            _customButton.OnClick += OnDoubleCliqued;
        }

        private void OnDestroy()
        {
            _customButton.OnClick -= OnDoubleCliqued;
        }

        private void OnDoubleCliqued(UI_CustomButton _)
        {
            SetState(E_MailState.Read);
            OpenMailCliqued?.Invoke(_datas);
        }

        public void Initialize(MailDatas datas)
        {
            _datas = datas;
            _expeditorText.text = datas.ExpeditorName;
            _objectText.text = datas.MailObject;
            _arrivalDateText.text = datas.GetDate();
        }

        public void SetIcon(Sprite icon)
        {
            _mailStateImage.sprite = icon;
        }

        public void SetState(E_MailState nextState)
        {
            switch(nextState)
            {
                case E_MailState.Unread:
                    SetUnreadState();
                    break;
                case E_MailState.Read:
                    SetReadState();
                    break;
            }
        }

        private void SetUnreadState()
        {
            SetIcon(_unreadSprite);
            _state = E_MailState.Unread;
            _background.color = _unreadColor;
        }

        private void SetReadState()
        {
            SetIcon(_readSprite);
            _state = E_MailState.Read;
            _background.color = _readColor;
            _customButton.HoverColor = new Color(_readColor.r * 0.5f, _readColor.g * 0.5f, _readColor.b * 0.5f, 1.0f);
            _customButton.NormalColor = _readColor;
            _customButton.SelectedColor = new Color(_readColor.r * 0.8f, _readColor.g * 0.8f, _readColor.b * 0.8f, 1.0f);
        }
    }
}
