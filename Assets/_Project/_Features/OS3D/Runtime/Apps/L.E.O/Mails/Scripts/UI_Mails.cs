using System;
using System.Collections.Generic;
using UnityEngine;

namespace LightHouse.Features.Computer.LEO.Mails
{
    public class UI_Mails : LEOWindow
    {
        [SerializeField] private RectTransform _mailParent;
        [SerializeField] private UI_MailElement _mailPrefab;
        [SerializeField] private UI_MailPopup _mailPopup;

        private UI_MailPopup _currentMailPopupInstance;

        public List<UI_MailElement> AllMails = new List<UI_MailElement>();

        public void GenerateMail(MailDatas datas)
        {
            UI_MailElement newInstance = Instantiate(_mailPrefab, _mailParent);
            newInstance.Initialize(datas);
            newInstance.OpenMailCliqued += MailController_OnOpen;
            AllMails.Add(newInstance);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            foreach(var mail in AllMails)
            {
                mail.OpenMailCliqued -= MailController_OnOpen;
            }
            AllMails.Clear();
        }

        private void MailController_OnOpen(MailDatas datas)
        {
            _currentMailPopupInstance = Instantiate(_mailPopup, this.transform as RectTransform);
            _currentMailPopupInstance.Initialize(datas);
            _currentMailPopupInstance.OnMailReceiptCliqued += CurrentMailPopupInstance_OnMailReceiptCliqued;
        }

        private void CurrentMailPopupInstance_OnMailReceiptCliqued()
        {
            _currentMailPopupInstance.OnMailReceiptCliqued -= CurrentMailPopupInstance_OnMailReceiptCliqued;
            Destroy(_currentMailPopupInstance.gameObject);
            _currentMailPopupInstance = null;
        }
    }

}
